using HarmonyLib;
using MGSC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ProduceAsReady
{
    [HarmonyPatch(typeof(ItemProductionSystem), nameof(ItemProductionSystem.Update))]
    public static class ItemProductionSystem_Update_Patch
    {
        public static bool Prefix(MagnumCargo magnumCargo, SpaceTime spaceTime)
        {

            //Due to the amount of changes, this completely replaces the original method.

            foreach (KeyValuePair<int, List<ProduceOrder>> itemProduceOrder in magnumCargo.ItemProduceOrders)
            {
                if (itemProduceOrder.Value.Count == 0)
                {
                    continue;
                }
                ProduceOrder produceOrder = itemProduceOrder.Value[0];

                int totalOrderHours = produceOrder.DurationInHours;
                int hoursPerItem = totalOrderHours / produceOrder.Count;

                //Check if at least one item can be produced
                if (produceOrder.StartTime.AddHours(hoursPerItem) > spaceTime.Time)
                {
                    // Not enough time has passed to produce any items
                    continue;
                }


                // Calculate how many items can be produced based on elapsed time
                double elapsedHours = (spaceTime.Time - produceOrder.StartTime).TotalHours;
                int itemsProduced = (int)(elapsedHours / hoursPerItem);

                itemsProduced = Mathf.Min(itemsProduced, produceOrder.Count);

                for (int i = 0; i < itemsProduced; i++)
                {
                    BasePickupItem item = ItemProductionSystem.ProduceItem(produceOrder.OrderId);
                    MagnumCargoSystem.AddCargo(magnumCargo, spaceTime, item);
                }

                produceOrder.Count -= itemsProduced;
                produceOrder.StartTime = produceOrder.StartTime.AddHours(itemsProduced * hoursPerItem);
                produceOrder.DurationInHours -= itemsProduced * hoursPerItem;   //This is used for the UI.

                if (produceOrder.Count > 0)
                {
                    continue;
                }

                // Notify a full order was completed.
                // This will be incorrect since for long orders it is only the partial amount that was created
                UI.Staff.NotificationPanel.AddItemProducedNotify(produceOrder.OrderId, produceOrder.Count);

                itemProduceOrder.Value.RemoveAt(0);
                foreach (ProduceOrder item2 in itemProduceOrder.Value)
                {
                    //The finish time of the last order will already be computed.
                    //Use that as when the next order starts.  
                    //	NOTE: this fixes a minor issue with the vanilla game where it uses the current time instead
                    //  of when the production completed.  At normal speed it's not a big deal, but returning from 
                    //  the dungeon forwards time, and any high speed time jumps will make it more noticeable.
                    item2.StartTime = produceOrder.StartTime;

                }

                //From what I can tell, the next update loop handles the next order.  Maybe for UI update purposes?
            }

            return false;
        }
    }
}
