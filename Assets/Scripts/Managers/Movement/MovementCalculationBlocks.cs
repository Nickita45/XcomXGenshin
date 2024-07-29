using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace MovementAlgorimus
{
    public static class MovementCalculationBlocks
    {
        public static Dictionary<TerritroyReaded, TerritroyReaded> CalculateAllPossible(int countMove, Unit unit)
        {
            Dictionary<TerritroyReaded, TerritroyReaded> objectsCalculated = new();//the final version of list of territories

            Queue<(TerritroyReaded orig, TerritroyReaded previus)> nextCalculated = new();//stack for bottom up calculations
            nextCalculated.Enqueue((unit.ActualTerritory, null));//first element
            HashSet<TerritroyReaded> already = new(); //save all blocks that we already detect

            int startValueMove = 0;

            // charge
            if (unit is Character && !Manager.StatusMain.HasPermisson(Permissions.SummonObjectOnMap))
            {
                if (unit.ActionsLeft > 1)
                {
                    startValueMove = countMove;
                    countMove *= 2;
                }
            }


            for (int i = 0; i <= countMove; i++)
            {
                int countNeedToBeAnalyzed = nextCalculated.Count(); //get count blocks we need to detect
                while (countNeedToBeAnalyzed > 0) //while exists block that we do not detect
                {
                    --countNeedToBeAnalyzed;
                    (TerritroyReaded orig, TerritroyReaded previus) = nextCalculated.Dequeue();

                    if ((orig.TerritoryInfo != TerritoryType.Character || orig == unit.ActualTerritory) &&
                        orig.TerritoryInfo != TerritoryType.ShelterGround && orig.TerritoryInfo != TerritoryType.Enemy) //detect only block which we can move on
                    {
                        if (objectsCalculated.ContainsKey(orig))
                        {
                            var oldItem = objectsCalculated[orig]; // this part of the code makes it more human
                            if (oldItem != null)
                            {
                                if ((oldItem.YPosition != orig.YPosition && previus.YPosition == orig.YPosition) ||
                                    (oldItem.YPosition != orig.YPosition && previus.YPosition != orig.YPosition &&
                                       Vector3.Distance(unit.transform.localPosition, previus.GetCordinats()) < Vector3.Distance(unit.transform.localPosition, oldItem.GetCordinats())))
                                { //solves the problem with not nessary jumps
                                    objectsCalculated.Remove(orig);
                                    objectsCalculated.Add(orig, previus);

                                    //change color if it is charge block or summon block
                                    //mb move to method?
                                    if (unit is Character)
                                    {
                                        Manager.Map.GetAirPlatform(orig).GetComponent<PlateMoving>()
                                                .SetType(i > startValueMove ? PlateMovingType.Charge : PlateMovingType.Usual);
                                    }
                                }
                            }
                        }
                        else
                        {
                            objectsCalculated.Add(orig, previus);

                            //change color if it is charge block or summon block
                            //mb move to method?
                            if (unit is Character)
                            {
                                Manager.Map.GetAirPlatform(orig).GetComponent<PlateMoving>()
                                    .SetType(i > startValueMove ? PlateMovingType.Charge : PlateMovingType.Usual);
                            }
                        }
                    }

                    foreach (var item in orig)// detect all neighbors
                    {

                        if (AddingValidationToCalculationList(item, objectsCalculated, already, orig, out TerritroyReaded detectItem))
                        {
                            nextCalculated.Enqueue((detectItem, orig));

                            if (!already.Contains(detectItem))
                                already.Add(detectItem);
                        }
                    }
                }

            }
            return objectsCalculated;
        }

        public static Dictionary<TerritroyReaded, TerritroyReaded> CalculateAllPossibleInSquare(int countMove, Unit unit)
        {
            Dictionary<TerritroyReaded, TerritroyReaded> objectsCalculated = new();//the final version of list of territories

            Queue<(TerritroyReaded orig, TerritroyReaded previus)> nextCalculated = new();//stack for bottom up calculations
            nextCalculated.Enqueue((unit.ActualTerritory, null));//first element
            HashSet<TerritroyReaded> already = new(); //save all blocks that we already detect

            for (int i = 0; i <= countMove; i++)
            {
                int countNeedToBeAnalyzed = nextCalculated.Count(); //get count blocks we need to detect
                while (countNeedToBeAnalyzed > 0) //while exists block that we do not detect
                {
                    --countNeedToBeAnalyzed;
                    (TerritroyReaded orig, TerritroyReaded previus) = nextCalculated.Dequeue();

                    if (orig.TerritoryInfo == TerritoryType.Air)
                    {
                        if (!objectsCalculated.ContainsKey(orig))
                        {
                            objectsCalculated.Add(orig, previus);
                            Manager.Map.GetAirPlatform(orig).GetComponent<PlateMoving>().SetType(PlateMovingType.Summon);
                        }

                    }

                    foreach (var item in orig)// detect all neighbors
                    {
                        if (AddingValidationToCalculationList(item, objectsCalculated, already, orig, out TerritroyReaded detectItem))
                        {
                            nextCalculated.Enqueue((detectItem, orig));

                            if (!already.Contains(detectItem))
                                already.Add(detectItem);
                        }
                    }
                }

                HashSet<TerritroyReaded> savingHash = new();

                foreach ((TerritroyReaded orig, TerritroyReaded previus) in nextCalculated.ToList())
                {
                    foreach (var item in orig)// detect all neighbors
                    {
                        if (AddingValidationToCalculationList(item, objectsCalculated, already, orig, out TerritroyReaded detectItem))
                        {
                            if (savingHash.Contains(detectItem))
                                nextCalculated.Enqueue((detectItem, orig));
                            else
                                savingHash.Add(detectItem);
                        }
                    }
                }
            }

            return objectsCalculated;
        }

        private static bool AddingValidationToCalculationList(string item,
                      Dictionary<TerritroyReaded, TerritroyReaded> objectsCalculated,
                      HashSet<TerritroyReaded> already,
                      TerritroyReaded orig,
                      out TerritroyReaded detectItem
            )
        {
            detectItem = Manager.Map[item];

            //optimization
            if (objectsCalculated.ContainsKey(detectItem) || already.Contains(detectItem))
                return false;

            if (detectItem.TerritoryInfo == TerritoryType.ShelterGround) //if we detect Shelter Ground, set detectItem as air above it 
            {
                var gettedString = detectItem.IndexUp.OrderBy(n => Vector3.Distance(TerritroyReaded.MakeVectorFromIndex(orig.Index), TerritroyReaded.MakeVectorFromIndex(n)))
                    .FirstOrDefault(n => Vector3.Distance(TerritroyReaded.MakeVectorFromIndex(orig.Index), TerritroyReaded.MakeVectorFromIndex(n)) < 2);

                if (gettedString == null) return false;
                detectItem =
                    Manager.Map[gettedString];

            }
            if (detectItem.IndexDown.Count(n => Manager.Map[n].TerritoryInfo == TerritoryType.Air) == 1)//if there's air under the block, it's going down.
            {
                var newItem = detectItem.IndexDown.FirstOrDefault(n => Manager.Map[n].TerritoryInfo == TerritoryType.Air);
                //select the bottommost block
                while (Manager.Map[newItem].IndexDown.Count(n => Manager.Map[n].TerritoryInfo == TerritoryType.Air) == 1)
                    newItem = Manager.Map[newItem].IndexDown.FirstOrDefault(n => Manager.Map[n].TerritoryInfo == TerritoryType.Air);

                detectItem = Manager.Map[newItem];
            }
            // we dont select such blocks with such rules
            if (detectItem.TerritoryInfo == TerritoryType.Shelter || detectItem.TerritoryInfo == TerritoryType.ShelterGround
                || detectItem.TerritoryInfo == TerritoryType.Enemy || detectItem.TerritoryInfo == TerritoryType.Character ||
              detectItem.IndexDown.Count(n => Manager.Map[n].TerritoryInfo == TerritoryType.Ground ||
              Manager.Map[n].TerritoryInfo == TerritoryType.ShelterGround) == 0)
                return false;

            //already detected block
            if (objectsCalculated.ContainsKey(detectItem))
                return false;

            //optimization
            if (already.Contains(detectItem) && (!detectItem.IsNearIsGround() || detectItem.InACenterOfGronds()))
                return false;

            return true;
        }
    }
}
