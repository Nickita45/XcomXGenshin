using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace MovementAlgorimus
{
    public static class MovementCalculationPaths
    {
        // vector with max value of Integer
        public static readonly Vector3 BIGVECTOR = new(int.MaxValue, int.MaxValue, int.MaxValue);

        public static List<Vector3> CalculateAllPath(TerritroyReaded starter, Unit unit, Dictionary<TerritroyReaded, TerritroyReaded> objectsCaclucated)
        {
            List<Vector3> path = new() //list of all point "breaks"
        {
            unit.transform.localPosition, //first point is unit location
            starter.GetCordinats() //selected block
        };

            Dictionary<Vector3, Vector3> airPaths = new(); //list of points of ascents and descents(air)

            while (true) //in this cyklus we will find all points "breaks"
            {
                List<Vector3> newPath = new(); //create new list of points
                for (int i = 0; i < path.Count - 1; i++) //we detect, if the between points exist shelters

                {
                    var iList = CalculatePoints(Manager.Map[path[i + 1]], path[i], objectsCaclucated);
                    newPath.AddRange(iList.paths);//if yes, we add them to list
                    foreach (var item in iList.airPaths) // and add air points
                    {
                        airPaths.TryAdd(item.Key, item.Value);
                    }
                }

                newPath = newPath.Distinct().ToList();//delete all same pieces
                if (newPath.Count == path.Count)//if old path and new path has same count, it means that we cant find new brakes
                    break;

                path = new List<Vector3>(newPath);
            }

            var basicPaths = FindPathBack(starter, objectsCaclucated); //detect all path from begin to end with their indexes
            int indexes = basicPaths.Count + 1; //to first one
            Vector3[] finalCordinats = new Vector3[indexes + airPaths.Count]; //create array to make consistent path
            Array.Fill(finalCordinats, BIGVECTOR);

            if (path.Count == 0)
            {
                return path;
            }

            finalCordinats[indexes + airPaths.Count - 1] = path.First();//set first element, because for reserve
            int airIndex = airPaths.Count; // index for seting path in raw

            foreach (var item in path)
            {
                if (airPaths.ContainsKey(item)) //if here is air
                {
                    finalCordinats[basicPaths[item] + airIndex] = airPaths[item]; //add air
                    airPaths.Remove(item);
                    airIndex--; // after add next item
                }
                finalCordinats[basicPaths[item] + airIndex] = item;
            }
            var endList = finalCordinats.Where(n => n != BIGVECTOR).Distinct().Reverse().ToList();

            return endList;
        }

        private static (List<Vector3> paths, Dictionary<Vector3, Vector3> airPaths) CalculatePoints(TerritroyReaded starter, Vector3 firstVector, Dictionary<TerritroyReaded, TerritroyReaded> objectsCaclucated)
        {

            Dictionary<Vector3, int> paths = FindPathBack(starter, objectsCaclucated); //find all path from starter to aktual player (path is territories with their numeration)
            int indexes = paths.Count + 1;//spesial indexer for future sort path

            Vector3 targetPosition = starter.GetCordinats() - firstVector;
            RaycastHit[] hits = Physics.RaycastAll(firstVector + Manager.MainParent.transform.position, targetPosition, Vector3.Distance(firstVector, starter.GetCordinats()));

            Debug.DrawRay(firstVector + Manager.MainParent.transform.position, targetPosition, Color.red);

            Dictionary<Vector3, int> nextPaths = new(); //the points where we need to make break for line

            Dictionary<Vector3, Vector3> airPaths = new(); //the points with air paths

            foreach (var item in paths) //in this cycle we detect all air breaks (only for shelter ground)
            {
                var aktualItem = Manager.Map[item.Key];
                var beforeItem = objectsCaclucated[aktualItem]; //get previus block

                if (beforeItem == null || aktualItem == null) //if we in the beggining (cant find previous or next block)
                    continue;

                if (beforeItem.YPosition != aktualItem.YPosition && beforeItem.HasGround() && aktualItem.HasGround())
                { //if it is true => we find air break
                    TerritroyReaded newAir = null;//detected Air territory
                    if (beforeItem.YPosition < aktualItem.YPosition) //up or down air break
                    {
                        newAir = Manager.Map[beforeItem.IndexUp.First()];
                    }
                    else if (beforeItem.YPosition > aktualItem.YPosition)
                    {
                        newAir = Manager.Map[aktualItem.IndexUp.First()];
                    }

                    if (newAir != null)
                    { //if we find new air break, add it to our air breaks
                        nextPaths.TryAdd(item.Key, item.Value);
                        nextPaths.TryAdd(beforeItem.GetCordinats(), paths[beforeItem.GetCordinats()]);
                        airPaths.TryAdd(aktualItem.GetCordinats(), newAir.GetCordinats());
                    }
                }
            }

            foreach (RaycastHit hit in hits) // check all hits
            {
                GameObject hitObject = hit.collider.gameObject;
                if (!hitObject.GetComponent<TerritoryInfo>())
                    continue;

                if (hitObject.name == "NoParent")
                    hitObject = hitObject.transform.parent.gameObject;

                TerritroyReaded finded = Manager.Map[hitObject.transform.localPosition];//find hit territory
                Queue<TerritroyReaded> territoryes = new(); //Queue for new territories, which we must detect
                HashSet<TerritroyReaded> alreadyFinded = new(); //This hashSet we use to optimation our future calculations
                territoryes.Enqueue(finded); // first territory

                bool doesFindSomething = false; // stop boolean
                while (true) // in this cycle we search from hit for any territory which is in our path and
                {
                    do
                    {
                        TerritroyReaded newFinded = territoryes.Dequeue();
                        alreadyFinded.Add(newFinded);

                        foreach (var item in newFinded) //analyze all neighbors
                        {
                            TerritroyReaded detectItem = Manager.Map[item];
                            if (alreadyFinded.Contains(detectItem) || territoryes.Contains(detectItem))//IT CAN BE MAKED FASTER O(n)!!!! territoryes is a queue and contains is O(n) operation
                                continue;

                            if (paths.ContainsKey(detectItem.GetCordinats()))
                            {
                                nextPaths.TryAdd(detectItem.GetCordinats(), paths[detectItem.GetCordinats()]);

                                doesFindSomething = true; //stop algoritmus after all cycle

                                foreach (var nextItem in detectItem) // detect also neighbors' neigbors
                                {
                                    TerritroyReaded nextDetectedItem = Manager.Map[nextItem];

                                    if (paths.ContainsKey(nextDetectedItem.GetCordinats()))
                                    {
                                        nextPaths.TryAdd(nextDetectedItem.GetCordinats(), paths[nextDetectedItem.GetCordinats()]);
                                    }
                                }
                            }
                            else
                            {
                                territoryes.Enqueue(detectItem);
                            }
                        }
                    }
                    while (!doesFindSomething && territoryes.Count != 0);

                    if (doesFindSomething)
                        break;
                }
            }

            //the final phase of the algoritmus
            Vector3[] finalCordinats = new Vector3[indexes];//create array to make new numeration
            Array.Fill(finalCordinats, BIGVECTOR);

            finalCordinats[indexes - 1] = firstVector;//set last as first, because reserve in the finish of algoritmus
            foreach (var item in nextPaths) //set all points in which we need to make stops
            {
                finalCordinats[item.Value] = item.Key;
            }

            var endList = finalCordinats.Where(n => n != BIGVECTOR).Distinct().Reverse().ToList();//clear from BIGVECTOR and reserve the array
            endList.Add(starter.GetCordinats());

            return (endList, airPaths);
        }

        private static Dictionary<Vector3, int> FindPathBack(TerritroyReaded starter, Dictionary<TerritroyReaded, TerritroyReaded> objectsCaclucated, TerritroyReaded begin = null)
        {
            Dictionary<Vector3, int> paths = new(); //index from high to below
            TerritroyReaded aktual = starter;
            int indexes = 0;
            while (aktual != begin)
            {
                paths.Add(aktual.GetCordinats(), indexes++);

                aktual = objectsCaclucated[aktual];
            }
            return paths; //paths is a dictionary with a way from starter to begin with indexers
        }

    }
}