using Adventure.Application.Rewards;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Adventure.Presentation.Rewards
{
    public sealed class BattleRewardItemView : MonoBehaviour
    {
        [SerializeField] private Image iconImage;
        [SerializeField] private TextMeshProUGUI amountText;

        public void Bind(BattleRewardItemViewData viewData)
        {
            if (iconImage != null)
            {
                iconImage.sprite = viewData.Icon;
                iconImage.enabled = viewData.Icon != null;
            }

            if (amountText != null)
            {
                amountText.text = $"x{viewData.Amount}";
            }
        }
    }
}
