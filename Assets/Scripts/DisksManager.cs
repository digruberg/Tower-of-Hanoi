﻿using System.Collections.Generic;
using UnityEngine;
using System;

namespace TowerHanoi
{
    /// <summary>
    /// Manage a work of resolve the puzzle
    /// </summary>
    public class DisksManager : MonoBehaviour
    {
        //setting
        [Range(0.1f, 0.9f)]
        [SerializeField]
        float minScaleDisk = 0.3f;
        [Range(2, 16)]
        int countDisks = 5;

        //links
        [SerializeField] Disk prefabDisk;
        [SerializeField] StockDisks stockSource;
        [SerializeField] StockDisks stockDestination;
        [SerializeField] StockDisks stockBuffer;

        //private date
        List<DirectionReplace> replacementMap = new List<DirectionReplace>();
        Action callBack;
        Disk activeDisk;
        int indexStepOnMap;

        //public data
        /// <summary>
        /// How many disks in system
        /// </summary>
        public int CountDisks
        {
            get
            {
                return countDisks;
            }

            set
            {
                countDisks = value;
            }
        }
        [HideInInspector] public bool isWorking;
        /// <summary>
        /// How many steps have the work
        /// </summary>
        public int CountStepsOfWork
        {
            get
            {
                return replacementMap.Count;
            }
        }
        /// <summary>
        /// Number current step of work
        /// </summary>
        public int CurrentStepOfWork
        {
            get
            {
                return indexStepOnMap + 1;
            }
        }

        //events
        /// <summary>
        ///  Called when a disk complete his move
        /// </summary>
        public event Action OnCompleteMoveCircle;

        #region public commands
        /// <summary>
        /// Stop any work and Destroy all disks
        /// </summary>
        public void StopAndClear()
        {
            if (isWorking)
            {
                activeDisk.Cancel();
                Destroy(activeDisk.gameObject);
            }
            isWorking = false;
            stockSource.Clear();
            stockDestination.Clear();
            stockBuffer.Clear();
            replacementMap.Clear();
        }
        /// <summary>
        /// Calculate how to solve the puzzle. Start work.
        /// </summary>
        /// <param name="_callBack">will be invoke after finish the work</param>
        public void StartWork(Action _callBack)
        {
            StopAndClear();
            callBack = _callBack;
            isWorking = true;
            indexStepOnMap = 0;
            InstanceDisks();
            CalculateReplacementMap(stockSource, stockBuffer, stockDestination, CountDisks);
            StartNextMove(replacementMap[indexStepOnMap].stockA, replacementMap[indexStepOnMap].stockB);
        }
        /// <summary>
        /// Set a new role for each stock
        /// </summary>
        /// <param name="A">stock will set as source</param>
        /// <param name="B">stock will set as destination</param>
        /// <param name="C">stock will set as buffer</param>
        public void OverrideStocksDestination(StockDisks A, StockDisks B, StockDisks C)
        {
            stockSource = A;
            stockDestination = B;
            stockBuffer = C;
        }
        #endregion

        #region other methods
        void InstanceDisks()
        {
            float scale_factor = 1;
            float step_scale_down = 1f / CountDisks;
            for (int i = 0; i < CountDisks; i++)
            {
                scale_factor = Mathf.Lerp(1, minScaleDisk, (float)i / CountDisks);
                Disk instance = Instantiate(prefabDisk);
                instance.transform.localScale = new Vector3(instance.transform.localScale.x * scale_factor, instance.transform.localScale.y, instance.transform.localScale.z * scale_factor);
                stockSource.Add(instance);
            }
        }
        void CalculateReplacementMap(StockDisks start, StockDisks temp, StockDisks end, int disks)
        {
            if (disks > 1)
                CalculateReplacementMap(start, end, temp, disks - 1);
            replacementMap.Add(new DirectionReplace(start, end));
            if (disks > 1)
                CalculateReplacementMap(temp, start, end, disks - 1);
        }
        void StartNextMove(StockDisks from, StockDisks to)
        {
            activeDisk = from.Remove();
            Vector3 destination = to.GetLastFreePos();
            float overPos = Mathf.Max(stockSource.GetLastFreePos().y, stockBuffer.GetLastFreePos().y, stockDestination.GetLastFreePos().y);
            activeDisk.StartMoveTo(destination, overPos,() =>
            {
                to.Add(activeDisk);

                indexStepOnMap++;
            //if work is done, than finish work
            if (replacementMap.Count == indexStepOnMap)
                {
                    isWorking = false;
                    callBack();
                    return;
                }

                if (OnCompleteMoveCircle != null)
                    OnCompleteMoveCircle();

            //start next replace of circle
            StartNextMove(replacementMap[indexStepOnMap].stockA, replacementMap[indexStepOnMap].stockB);
            });
        }
        #endregion

        struct DirectionReplace
        {
            public DirectionReplace(StockDisks A, StockDisks B)
            {
                stockA = A;
                stockB = B;
            }
            public StockDisks stockA;
            public StockDisks stockB;
        }
    }
}