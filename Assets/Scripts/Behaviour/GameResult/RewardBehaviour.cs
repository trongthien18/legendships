﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class RewardBehaviour : MonoBehaviour
{
    public PlayerStatsBehaviour playerStatsScript;
    public Text textGoldReward;
    public Text textExpReward;
    public Text textRankReward;

    float timer;

	void Start ()
    {
        int position = -1;
        if (PhotonNetwork.player.customProperties.ContainsKey("POSITION"))
        {
            position = (int)PhotonNetwork.player.customProperties["POSITION"];
        }
        
        int goldReward = GetGoldReward(position);
        int expReward = GetExpReward(position);
        int rankReward = GetRankReward(position);

        textGoldReward.text = goldReward.ToString();
        textExpReward.text = expReward.ToString();
        textRankReward.text = rankReward.ToString();

        PlayerData.Current.elo += rankReward;
        PlayerData.Current.played += 1;
        if (position <= 1)
            PlayerData.Current.win += 1;
        CoinController.Instance.SetCoins(PlayerData.Current.gold + goldReward);
        playerStatsScript.SetExpBonus(expReward);
	}

    public int GetGoldReward(int position)
    {
        int result = 0;
        int playerCount = 1;
        if (PhotonNetwork.room != null)
        {
            playerCount = PhotonNetwork.room.playerCount;
        }

        if (playerCount <= 1 || PhotonNetwork.offlineMode == true || position == -1)
            return 0;

        switch (position)
        {
            case 1:
                result = 200 + playerCount * 10;
                break;
            case 2:
                result = 100 + playerCount * 10;
                break;
            case 3:
                result = 50 + playerCount * 10;
                break;
            case 4:
                result = 20 + playerCount * 10;
                break;
        }

        return result;
    }

    public int GetExpReward(int position)
    {
        int result = 0;
        int playerCount = 1;
        if (PhotonNetwork.room != null)
        {
            playerCount = PhotonNetwork.room.playerCount;
        }

        if (playerCount <= 1 || PhotonNetwork.offlineMode == true || position == -1)
            return 0;

        switch (position)
        {
            case 1:
                result = 100 + playerCount * 10;
                break;
            case 2:
                result = 50 + playerCount * 10;
                break;
            case 3:
                result = 20 + playerCount * 10;
                break;
            case 4:
                result = 10 + playerCount * 10;
                break;
        }

        return result;
    }

    public int GetRankReward(int position)
    {
        int playerCount = 1;
        if (PhotonNetwork.room != null)
        {
            playerCount = PhotonNetwork.room.playerCount;
        }

        if (playerCount <= 1 || PhotonNetwork.offlineMode == true || position == -1)
            return 0;

        return 4 - position;
    }
}
