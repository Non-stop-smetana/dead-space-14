// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Content.Shared.Actions;
using Content.Shared.Clothing;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Storage;
using Content.Shared.Inventory;
using Robust.Shared.Containers;
using Robust.Shared.Prototypes;

namespace Content.Server.Clothing;

public sealed class HolsterDrawSystem : EntitySystem
{
    [Dependency] private readonly SharedHandsSystem _hands = default!;
    [Dependency] private readonly SharedContainerSystem _container = default!;
    private const string RevolverProtoId = "WeaponRevolverBlueShieldKR19";

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<HolsterDrawComponent, GetItemActionsEvent>(OnGetActions);

        SubscribeLocalEvent<HolsterDrawComponent, HolsterDrawActionEvent>(OnDrawAction);
    }

    private void OnGetActions(Entity<HolsterDrawComponent> ent, ref GetItemActionsEvent args)
    {
        if (args.SlotFlags != SlotFlags.BELT)
            return;

        args.AddAction(ref ent.Comp.ActionEntity, ent.Comp.ActionProto.Id, ent.Owner);
    }

    private void OnDrawAction(Entity<HolsterDrawComponent> ent, ref HolsterDrawActionEvent args)
    {
        if (args.Handled)
            return;

        var user = args.Performer;
        if (!TryComp<StorageComponent>(ent, out var storage))
            return;
        var revolvers = new List<EntityUid>();
        foreach (var item in storage.Container.ContainedEntities)
        {
            var meta = MetaData(item);
            if (meta.EntityPrototype?.ID == RevolverProtoId)
                revolvers.Add(item);
        }
        if (revolvers.Count == 0)
            return;
        foreach (var rev in revolvers)
        {
            _container.Remove(rev, storage.Container);
        }
        if (revolvers.Count == 1)
        {
            var activeHand = _hands.GetActiveHand(user);
            _hands.TryPickup(user, revolvers[0], activeHand);
        }
        else
        {
            var activeHand = _hands.GetActiveHand(user);
            _hands.TryPickup(user, revolvers[0], activeHand);

            if (_hands.TryGetEmptyHand(user, out var secondHand))
            {
                _hands.TryPickup(user, revolvers[1], secondHand);
            }
            else
            {
                _hands.TryPickupAnyHand(user, revolvers[1]);
            }
        }

        args.Handled = true;
    }
}