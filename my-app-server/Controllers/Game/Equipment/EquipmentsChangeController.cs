using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using my_app_server.Models;

namespace my_app_server.Controllers
{
    [Produces("application/json")]
    [Route("api/EquipmentsChange")]
    public class EquipmentsChangeController : Controller
    {
        private readonly my_appContext _context;

        public EquipmentsChangeController(my_appContext context)
        {
            _context = context;
        }

        // POST: api/EquipmentsChange
        [HttpPost]
        public async Task<IActionResult> PostEquipment([FromBody] PassedGameData<PassedChangeEqData> passedData)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            DateTime now = DateTime.UtcNow;
            if (passedData.UserToken == null || passedData.ActionToken == null)
            {
                return BadRequest(new DataError("securityErr", "No authorization controll."));
            }
            UserToken dbtoken = Security.CheckUserToken(this._context, passedData.UserToken);
            if (dbtoken == null)
            {
                return BadRequest(new DataError("securityErr", "Your data has probably been stolen or modified manually. We suggest password's change."));
            }
            else
            {
                if (!dbtoken.IsTimeValid(now))
                {
                    return BadRequest(new DataError("timeoutErr", "You have been too long inactive. Relogin is required."));
                }
                else
                {
                    dbtoken.UpdateToken(now);
                }
            }
            Heros hero = _context.Heros.FirstOrDefault(e => e.Name == passedData.ActionToken.HeroName);
            ActionToken gametoken = Security.CheckActionToken(_context, passedData.ActionToken, hero.HeroId);
            if (gametoken == null)
            {
                return BadRequest(new DataError("securityErr", "Your data has probably been stolen or modified manually. We suggest password's change."));
            }
            else
            {
                if (!gametoken.IsTimeValid(now))
                {
                    return BadRequest(new DataError("timeoutErr", "You have been too long inactive. Relogin is required."));
                }
                else
                {
                    gametoken.UpdateToken(now);
                }
            }
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                return BadRequest(new DataError("databaseErr", "Failed to update tokens."));
            }
            // can do stuff

            var Equipment = _context.Equipment.FirstOrDefault(e => e.HeroId == hero.HeroId);
            if (Equipment == null)
            {
                return BadRequest(new DataError("equipmentErr", "Hero is without equipment."));
            }
            var Backpack = _context.Backpack.Where(e => e.HeroId == hero.HeroId);

            var Added = new List<EquipmentModifyResult.EquipmentModification>();
            var Removed = new List<EquipmentModifyResult.EquipmentModification>();

            if (passedData.Data.From == passedData.Data.To)
            {
                return Ok(new { success = true, changes = new EquipmentModifyResult() { Added = Added.ToArray(), Removed = Removed.ToArray() } });
            }
            // BP -> BP
            if (passedData.Data.From.StartsWith("Backpack") && passedData.Data.To.StartsWith("Backpack"))
            {
                int fromSlot = int.Parse(passedData.Data.From.Substring(8));
                int toSlot = int.Parse(passedData.Data.To.Substring(8));
                var fromItem = _context.Backpack.FirstOrDefault(e => e.HeroId == hero.HeroId && e.SlotNr == fromSlot);
                if (fromItem == null)
                {
                    return BadRequest(new DataError("changeEqErr", "No initial item."));
                }
                // remove from current and add to new
                Removed.Add(new EquipmentModifyResult.EquipmentModification() { ItemID = null, Target = passedData.Data.From });
                Added.Add(new EquipmentModifyResult.EquipmentModification() { ItemID = fromItem.ItemId, Target = passedData.Data.To });

                var toItem = _context.Backpack.FirstOrDefault(e => e.HeroId == hero.HeroId && e.SlotNr == toSlot);
                if (toItem == null)
                {
                    _context.Backpack.Remove(fromItem);
                    _context.Backpack.Add(new Backpack()
                    {
                        HeroId = hero.HeroId,
                        ItemId = fromItem.ItemId,
                        SlotNr = toSlot,
                    });
                }
                else
                {
                    Removed.Add(new EquipmentModifyResult.EquipmentModification() { ItemID = null, Target = passedData.Data.To });
                    Added.Add(new EquipmentModifyResult.EquipmentModification() { ItemID = toItem.ItemId, Target = passedData.Data.From });
                    int mem = toItem.ItemId;
                    toItem.ItemId = fromItem.ItemId;
                    fromItem.ItemId = mem;
                }
            }
            // BP -> EQ
            if (passedData.Data.From.StartsWith("Backpack") && passedData.Data.To.StartsWith("Inventory"))
            {
                int fromSlot = int.Parse(passedData.Data.From.Substring(8));
                int toSlot = int.Parse(passedData.Data.To.Substring(9));
                var fromItem = _context.Backpack.FirstOrDefault(e => e.HeroId == hero.HeroId && e.SlotNr == fromSlot);
                if (fromItem == null)
                {
                    return BadRequest(new DataError("changeEqErr", "No initial item."));
                }
                var passeditem = _context.Items.FirstOrDefault(e => e.ItemId == fromItem.ItemId);
                if (passeditem == null)
                {
                    return BadRequest(new DataError("changeEqErr", "No initial item."));
                }
                if (!EquipmentControll.CanEquip(passeditem, hero, toSlot))
                {
                    return BadRequest(new DataError("changeEqErr", "Items requirements are not fullfilled."));
                }
                Removed.Add(new EquipmentModifyResult.EquipmentModification() { ItemID = null, Target = passedData.Data.From });
                Added.Add(new EquipmentModifyResult.EquipmentModification() { ItemID = fromItem.ItemId, Target = passedData.Data.To });

                int? Target = null;
                switch (toSlot)
                {
                    case 0:
                        Target = Equipment.Helmet;
                        break;
                    case 1:
                        Target = Equipment.Ring1;
                        break;
                    case 2:
                        Target = Equipment.Neckles;
                        break;
                    case 3:
                        Target = Equipment.Ring2;
                        break;
                    case 4:
                        Target = Equipment.Gloves;
                        break;
                    case 5:
                        Target = Equipment.Armour;
                        break;
                    case 6:
                        Target = Equipment.Bracelet;
                        break;
                    case 7:
                        Target = Equipment.FirstHand;
                        break;
                    case 8:
                        Target = Equipment.Trousers;
                        break;
                    case 9:
                        Target = Equipment.SecondHand;
                        break;
                    case 10:
                        Target = Equipment.Shoes;
                        break;
                }

                var toItem = Target;

                // update inventory
                switch (toSlot)
                {
                    case 0:
                        Equipment.Helmet = fromItem.ItemId;
                        break;
                    case 1:
                        Equipment.Ring1 = fromItem.ItemId;
                        break;
                    case 2:
                        Equipment.Neckles = fromItem.ItemId;
                        break;
                    case 3:
                        Equipment.Ring2 = fromItem.ItemId;
                        break;
                    case 4:
                        Equipment.Gloves = fromItem.ItemId;
                        break;
                    case 5:
                        Equipment.Armour = fromItem.ItemId;
                        break;
                    case 6:
                        Equipment.Bracelet = fromItem.ItemId;
                        break;
                    case 7:
                        Equipment.FirstHand = fromItem.ItemId;
                        break;
                    case 8:
                        Equipment.Trousers = fromItem.ItemId;
                        break;
                    case 9:
                        Equipment.SecondHand = fromItem.ItemId;
                        break;
                    case 10:
                        Equipment.Shoes = fromItem.ItemId;
                        break;
                }

                // update backpack
                if (toItem == null)
                {
                    _context.Backpack.Remove(fromItem);
                    
                } else
                {
                    Removed.Add(new EquipmentModifyResult.EquipmentModification() { ItemID = null, Target = passedData.Data.To });
                    Added.Add(new EquipmentModifyResult.EquipmentModification() { ItemID = toItem, Target = passedData.Data.From });
                    fromItem.ItemId = toItem.Value;
                }
            }
            // EQ -> BP
            if (passedData.Data.From.StartsWith("Inventory") && passedData.Data.To.StartsWith("Backpack"))
            {
                int fromSlot = int.Parse(passedData.Data.From.Substring(9));
                int toSlot = int.Parse(passedData.Data.To.Substring(8));
                int? Target = null;
                switch (fromSlot)
                {
                    case 0:
                        Target = Equipment.Helmet;
                        break;
                    case 1:
                        Target = Equipment.Ring1;
                        break;
                    case 2:
                        Target = Equipment.Neckles;
                        break;
                    case 3:
                        Target = Equipment.Ring2;
                        break;
                    case 4:
                        Target = Equipment.Gloves;
                        break;
                    case 5:
                        Target = Equipment.Armour;
                        break;
                    case 6:
                        Target = Equipment.Bracelet;
                        break;
                    case 7:
                        Target = Equipment.FirstHand;
                        break;
                    case 8:
                        Target = Equipment.Trousers;
                        break;
                    case 9:
                        Target = Equipment.SecondHand;
                        break;
                    case 10:
                        Target = Equipment.Shoes;
                        break;
                }
                var fromItem = Target;
                if (fromItem == null)
                {
                    return BadRequest(new DataError("changeEqErr", "No initial item."));
                }

                var toItem = _context.Backpack.FirstOrDefault(e => e.HeroId == hero.HeroId && e.SlotNr == toSlot);
                if(toItem != null)
                {
                    // slot is being used
                    var passeditem = _context.Items.FirstOrDefault(e => e.ItemId == toItem.ItemId);
                    if (passeditem == null)
                    {
                        return BadRequest(new DataError("changeEqErr", "No target item in DB."));
                    }
                    if (!EquipmentControll.CanEquip(passeditem, hero, fromSlot))
                    {
                        return BadRequest(new DataError("changeEqErr", "Items requirements are not fullfilled."));
                    }
                }
                
                Removed.Add(new EquipmentModifyResult.EquipmentModification() { ItemID = null, Target = passedData.Data.From });
                Added.Add(new EquipmentModifyResult.EquipmentModification() { ItemID = fromItem.Value, Target = passedData.Data.To });

                // null if prev was empty lub itemID if existed
                int? invRes = toItem?.ItemId;
                // update inventory
                switch (fromSlot)
                {
                    case 0:
                        Equipment.Helmet = invRes;
                        break;
                    case 1:
                        Equipment.Ring1 = invRes;
                        break;
                    case 2:
                        Equipment.Neckles = invRes;
                        break;
                    case 3:
                        Equipment.Ring2 = invRes;
                        break;
                    case 4:
                        Equipment.Gloves = invRes;
                        break;
                    case 5:
                        Equipment.Armour = invRes;
                        break;
                    case 6:
                        Equipment.Bracelet = invRes;
                        break;
                    case 7:
                        Equipment.FirstHand = invRes;
                        break;
                    case 8:
                        Equipment.Trousers = invRes;
                        break;
                    case 9:
                        Equipment.SecondHand = invRes;
                        break;
                    case 10:
                        Equipment.Shoes = invRes;
                        break;
                }

                // update backpack
                if (toItem == null)
                {
                    _context.Backpack.Add(new Backpack()
                    {
                        HeroId = hero.HeroId,
                        ItemId = fromItem.Value,
                        SlotNr = toSlot,
                    });
                }
                else
                {
                    Removed.Add(new EquipmentModifyResult.EquipmentModification() { ItemID = null, Target = passedData.Data.To });
                    Added.Add(new EquipmentModifyResult.EquipmentModification() { ItemID = toItem.ItemId, Target = passedData.Data.From });
                    toItem.ItemId = fromItem.Value;
                }
            }
            // EQ -> EQ
            if (passedData.Data.From.StartsWith("Inventory") && passedData.Data.To.StartsWith("Inventory"))
            {
                int fromSlot = int.Parse(passedData.Data.From.Substring(9));
                int toSlot = int.Parse(passedData.Data.To.Substring(9));
                int? Target = null;
                switch (fromSlot)
                {
                    case 0:
                        Target = Equipment.Helmet;
                        break;
                    case 1:
                        Target = Equipment.Ring1;
                        break;
                    case 2:
                        Target = Equipment.Neckles;
                        break;
                    case 3:
                        Target = Equipment.Ring2;
                        break;
                    case 4:
                        Target = Equipment.Gloves;
                        break;
                    case 5:
                        Target = Equipment.Armour;
                        break;
                    case 6:
                        Target = Equipment.Bracelet;
                        break;
                    case 7:
                        Target = Equipment.FirstHand;
                        break;
                    case 8:
                        Target = Equipment.Trousers;
                        break;
                    case 9:
                        Target = Equipment.SecondHand;
                        break;
                    case 10:
                        Target = Equipment.Shoes;
                        break;
                }
                var fromItem = Target;
                if (fromItem == null)
                {
                    return BadRequest(new DataError("changeEqErr", "No initial item."));
                }
                Target = null;
                switch (toSlot)
                {
                    case 0:
                        Target = Equipment.Helmet;
                        break;
                    case 1:
                        Target = Equipment.Ring1;
                        break;
                    case 2:
                        Target = Equipment.Neckles;
                        break;
                    case 3:
                        Target = Equipment.Ring2;
                        break;
                    case 4:
                        Target = Equipment.Gloves;
                        break;
                    case 5:
                        Target = Equipment.Armour;
                        break;
                    case 6:
                        Target = Equipment.Bracelet;
                        break;
                    case 7:
                        Target = Equipment.FirstHand;
                        break;
                    case 8:
                        Target = Equipment.Trousers;
                        break;
                    case 9:
                        Target = Equipment.SecondHand;
                        break;
                    case 10:
                        Target = Equipment.Shoes;
                        break;
                }
                var toItem = Target;

                var passeditem = _context.Items.FirstOrDefault(e => e.ItemId == fromItem.Value);
                if (passeditem == null)
                {
                    return BadRequest(new DataError("changeEqErr", "No target item in DB."));
                }
                if (!EquipmentControll.CanEquip(passeditem, hero, toSlot))
                {
                    return BadRequest(new DataError("changeEqErr", "Items requirements are not fullfilled."));
                }
                if(toItem != null)
                {
                    var passeditem2 = _context.Items.FirstOrDefault(e => e.ItemId == toItem.Value);
                    if (passeditem2 == null)
                    {
                        return BadRequest(new DataError("changeEqErr", "No target item in DB."));
                    }
                    if (!EquipmentControll.CanEquip(passeditem2, hero, fromSlot))
                    {
                        return BadRequest(new DataError("changeEqErr", "Items requirements are not fullfilled."));
                    }
                }

                Removed.Add(new EquipmentModifyResult.EquipmentModification() { ItemID = null, Target = passedData.Data.From });
                Added.Add(new EquipmentModifyResult.EquipmentModification() { ItemID = fromItem.Value, Target = passedData.Data.To });

                // update inventory
                switch (fromSlot)
                {
                    case 0:
                        Equipment.Helmet = toItem;
                        break;
                    case 1:
                        Equipment.Ring1 = toItem;
                        break;
                    case 2:
                        Equipment.Neckles = toItem;
                        break;
                    case 3:
                        Equipment.Ring2 = toItem;
                        break;
                    case 4:
                        Equipment.Gloves = toItem;
                        break;
                    case 5:
                        Equipment.Armour = toItem;
                        break;
                    case 6:
                        Equipment.Bracelet = toItem;
                        break;
                    case 7:
                        Equipment.FirstHand = toItem;
                        break;
                    case 8:
                        Equipment.Trousers = toItem;
                        break;
                    case 9:
                        Equipment.SecondHand = toItem;
                        break;
                    case 10:
                        Equipment.Shoes = toItem;
                        break;
                }
                switch (toSlot)
                {
                    case 0:
                        Equipment.Helmet = fromItem;
                        break;
                    case 1:
                        Equipment.Ring1 = fromItem;
                        break;
                    case 2:
                        Equipment.Neckles = fromItem;
                        break;
                    case 3:
                        Equipment.Ring2 = fromItem;
                        break;
                    case 4:
                        Equipment.Gloves = fromItem;
                        break;
                    case 5:
                        Equipment.Armour = fromItem;
                        break;
                    case 6:
                        Equipment.Bracelet = fromItem;
                        break;
                    case 7:
                        Equipment.FirstHand = fromItem;
                        break;
                    case 8:
                        Equipment.Trousers = fromItem;
                        break;
                    case 9:
                        Equipment.SecondHand = fromItem;
                        break;
                    case 10:
                        Equipment.Shoes = fromItem;
                        break;
                }
                if(toItem!=null)
                {
                    Removed.Add(new EquipmentModifyResult.EquipmentModification() { ItemID = null, Target = passedData.Data.To });
                    Added.Add(new EquipmentModifyResult.EquipmentModification() { ItemID = toItem.Value, Target = passedData.Data.From });
                }
            }
            // Remove item
            if(passedData.Data.To == "Trash")
            {
                if (passedData.Data.From.StartsWith("Backpack"))
                {
                    int fromSlot = int.Parse(passedData.Data.From.Substring(8));
                    var fromItem = _context.Backpack.FirstOrDefault(e => e.HeroId == hero.HeroId && e.SlotNr == fromSlot);
                    if (fromItem == null)
                    {
                        return BadRequest(new DataError("changeEqErr", "No initial item."));
                    }
                    // remove from current and add to new
                    _context.Backpack.Remove(fromItem);
                    Removed.Add(new EquipmentModifyResult.EquipmentModification() { ItemID = null, Target = passedData.Data.From });
                }
                if (passedData.Data.From.StartsWith("Inventory"))
                {
                    int fromSlot = int.Parse(passedData.Data.From.Substring(9));
                    switch (fromSlot)
                    {
                        case 0:
                            Equipment.Helmet = null;
                            break;
                        case 1:
                            Equipment.Ring1 = null;
                            break;
                        case 2:
                            Equipment.Neckles = null;
                            break;
                        case 3:
                            Equipment.Ring2 = null;
                            break;
                        case 4:
                            Equipment.Gloves = null;
                            break;
                        case 5:
                            Equipment.Armour = null;
                            break;
                        case 6:
                            Equipment.Bracelet = null;
                            break;
                        case 7:
                            Equipment.FirstHand = null;
                            break;
                        case 8:
                            Equipment.Trousers = null;
                            break;
                        case 9:
                            Equipment.SecondHand = null;
                            break;
                        case 10:
                            Equipment.Shoes = null;
                            break;
                    }
                    Removed.Add(new EquipmentModifyResult.EquipmentModification() { ItemID = null, Target = passedData.Data.From });
                }
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                return BadRequest(new DataError("databaseErr", "Failed to update equipment."));
            }
            return Ok(new { success = true, changes = new EquipmentModifyResult() { Added = Added.ToArray(), Removed = Removed.ToArray() } });
        }
    }
    public class PassedChangeEqData
    {
        public string From { get; set; }
        public string To { get; set; }
    }
}