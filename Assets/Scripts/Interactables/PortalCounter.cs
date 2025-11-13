using UnityEngine;

namespace TinyChef
{
    public enum PortalColor
    {
        Red,
        Blue,
        Green,
        Yellow,
        Purple,
        Orange
    }

    public class PortalCounter : BaseCounter
    {
        [Header("Portal Settings")]
        [SerializeField] private PortalColor portalColor = PortalColor.Red;
        [SerializeField] private PortalCounter pairedPortal;

        public PortalColor Color => portalColor;
        public PortalCounter PairedPortal => pairedPortal;

        private void Awake()
        {
            counterType = CounterType.Portal;
        }

        public void SetPairedPortal(PortalCounter portal)
        {
            pairedPortal = portal;
        }

        public override void Interact()
        {
            if (pairedPortal == null)
            {
                Debug.LogWarning($"PortalCounter {gameObject.name} has no paired portal!");
                return;
            }

            Chef chef = FindObjectOfType<Chef>();
            if (chef == null)
            {
                Debug.LogWarning("No Chef found in scene!");
                return;
            }

            CounterGroup targetGroup = pairedPortal.GetComponentInParent<CounterGroup>();
            if (targetGroup == null)
            {
                Debug.LogWarning($"Paired portal {pairedPortal.gameObject.name} is not in a CounterGroup!");
                return;
            }

            chef.TeleportToPortal(pairedPortal, targetGroup);
        }

        public override void Process()
        {
            // Portals don't process items
        }

        protected override bool CanPlaceItem(IItem item)
        {
            // Portals don't accept items
            return false;
        }

        protected override bool CanProcess(IItem item)
        {
            // Portals don't process items
            return false;
        }
    }
}

