using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace OfferScreen
{
    public class DesignCard : MonoBehaviour, IOffer
    {
        public bool isLocked = false;

        [SerializeField] private Color lockedColor;
        [SerializeField] private Image cardBackgroundImage;
        [SerializeField] private CostDisplay costDisplay;
        [SerializeField] private CostDisplay mergeButton;
        [SerializeField] private LockButton lockButton;
        
        private DesignInfo _designInfo;
        private DesignCard _mergeableDesignCard;
        private IOffer _offerImplementation;
        private bool _canBeLocked = false;
        private bool _pointerInside = false;

        public Design Design => _designInfo.design;
        private bool HasMergeableDesignCard => _mergeableDesignCard != null;
        
        private void Awake()
        {
            _designInfo = GetComponentInChildren<DesignInfo>();
        }

        private void Start()
        {
            OfferScreenEvents.Current.OnGridsUpdated += CardsUpdated;
        }
        
        private void OnDestroy()
        {
            OfferScreenEvents.Current.OnGridsUpdated -= CardsUpdated;
        }
        
        public void OnPointerEnter(PointerEventData eventData)
        {
            _pointerInside = true;
            
            if (_canBeLocked)
            {
                lockButton.Show();
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _pointerInside = false;
            lockButton.Hide();
        }

        public void HideButtons()
        {
            mergeButton.gameObject.SetActive(false);
            _canBeLocked = false;
            lockButton.Hide();
        }

        public void PreventLocking()
        {
            isLocked = false;
            _canBeLocked = false;
        }

        public void AllowLocking()
        {
            _canBeLocked = true;

            if (_pointerInside)
            {
                lockButton.Show();
            }
        }

        public void Merge()
        {
            OfferManager.Current.TryMerge(_mergeableDesignCard, this);
        }

        public void ClickedLockButton()
        {
            isLocked = !isLocked;
            OfferScreenEvents.Current.RefreshOffers();
        }
        
        private void CardsUpdated()
        {
            _mergeableDesignCard = GetAnyMergeableDesignCard();
            mergeButton.gameObject.SetActive(HasMergeableDesignCard);
            if (HasMergeableDesignCard)
            {
                mergeButton.Refresh(Design.GetStat(St.Rarity));
            }

            _designInfo.Refresh();

            costDisplay.Refresh(Design.GetStat(St.Rarity));

            cardBackgroundImage.color = isLocked ? lockedColor : Color.white;
        }
        
        private DesignCard GetAnyMergeableDesignCard()
        {
            if (!Design.Upgradeable || Design.Level >= 2) return null;
            
            return OfferManager.Current.AllDesignCards
                .Where(d => d != this)
                .Where(d => d.Design.Title == Design.Title)
                .Where(d => !d.HasMergeableDesignCard)
                .FirstOrDefault(d => d.Design.Level < 2);
        }
    }
}
