﻿using Synthesis.FSM;
using Synthesis.GUI;
using Synthesis.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace Synthesis.States
{
    public class EnterInfoState : State
    {
        /// <summary>
        /// Joins the lobby with the given code and player tag when the join button
        /// is pressed.
        /// </summary>
        public void OnJoinButtonPressed()
        {
            string playerTag = GameObject.Find("PlayerTagText").GetComponent<Text>().text;

            if (playerTag.Length == 0)
            {
                UserMessageManager.Dispatch("Please enter a player tag.", 5f);
                return;
            }

            PlayerIdentity.DefaultLocalPlayerTag = playerTag;

            StateMachine.PushState(new LoadRobotState(
                new LobbyState(GameObject.Find("LobbyCodeText").GetComponent<Text>().text)));
        }

        /// <summary>
        /// Pops this <see cref="State"/> when the back button is pressed.
        /// </summary>
        public void OnBackButtonPressed()
        {
            StateMachine.PopState();
        }
    }
}
