﻿using UnityEngine;
using System.Collections.Generic;

public class ShopThemeList : ShopList
{
    public override void Populate()
    {
		m_RefreshCallback = null;
        foreach (Transform t in listRoot)
        {
            Destroy(t.gameObject);
        }

        foreach (KeyValuePair<string, ThemeData> pair in ThemeDatabase.dictionnary)
        {
            ThemeData theme = pair.Value;
            if (theme != null)
            {
                prefabItem.InstantiateAsync().Completed += (op) =>
                {
                    if (op.Result == null || !(op.Result is GameObject))
                    {
                        Debug.LogWarning(string.Format("Unable to load theme shop list {0}.", prefabItem.Asset.name));
                        return;
                    }
                    GameObject newEntry = op.Result;
                    newEntry.transform.SetParent(listRoot, false);

                    ShopItemListItem itm = newEntry.GetComponent<ShopItemListItem>();

                    itm.nameText.text = theme.themeName;
                    itm.pricetext.text = theme.cost.ToString();
                    itm.icon.sprite = theme.themeIcon;

                    if (theme.premiumCost > 0)
                    {
                        itm.premiumText.transform.parent.gameObject.SetActive(true);
                        itm.premiumText.text = theme.premiumCost.ToString();
                    }
                    else
                    {
                        itm.premiumText.transform.parent.gameObject.SetActive(false);
                    }

                    itm.buyButton.onClick.AddListener(delegate() { Buy(theme); });

                    itm.buyButton.image.sprite = itm.buyButtonSprite;

                    RefreshButton(itm, theme);
                    m_RefreshCallback += delegate() { RefreshButton(itm, theme); };
                };
            }
        }
    }

	protected void RefreshButton(ShopItemListItem itm, ThemeData theme)
	{
		if (theme.cost > PlayerDataWeb3.instance.User.Coins)
		{
			itm.buyButton.interactable = false;
			itm.pricetext.color = Color.red;
		}
		else
		{
			itm.pricetext.color = Color.black;
		}

		//if (theme.premiumCost > PlayerDataWeb3.instance.premium)
		//{
		//	itm.buyButton.interactable = false;
		//	itm.premiumText.color = Color.red;
		//}
		//else
		//{
		//	itm.premiumText.color = Color.black;
		//}

		if (PlayerDataWeb3.instance.themes.Contains(theme.themeName))
		{
			itm.buyButton.interactable = false;
			itm.buyButton.image.sprite = itm.disabledButtonSprite;
			itm.buyButton.transform.GetChild(0).GetComponent<UnityEngine.UI.Text>().text = "Owned";
		}
	}


	public void Buy(ThemeData t)
    {
        Web3Controller.ShowLoader("Claiming Theme");
        Web3Controller.ClaimTheme(t.themeName, t.cost)
            .OnFail(Web3Controller.ShowException)
            .Finally(() =>
            {
                Populate();
                Web3Controller.HideLoader();
            });
    }
}
