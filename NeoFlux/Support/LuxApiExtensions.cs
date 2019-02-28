using System;
using System.Linq;
using System.Numerics;
using LunarLabs.Parser;
using Neo.Lux.Core;
using Neo.Lux.Cryptography;
using Neo.Lux.Utils;
using NeoFlux.Model;

namespace NeoFlux.Support
{
    public class LuxApiExtensions
    {
        private NeoRPC RPC { get; }

        public LuxApiExtensions(NeoRPC neoRpc)
        {
            RPC = neoRpc;
        }

        public RawTransaction GetRawTransaction(String txId, bool findBlock)
        {
            DataNode transactionLog = RPC.QueryRPC("getapplicationlog", new Object[] {txId});
            if (transactionLog != null)
            {
                DataNode resultNode = transactionLog.GetNode("result");
                DataNode executions = resultNode.GetNode("executions");
                DataNode lastExecution = executions.GetNodeByIndex(executions.ChildCount - 1);
                DataNode notifications = lastExecution.GetNode("notifications");
                DataNode notificationData = notifications.GetNodeByIndex(notifications.ChildCount - 1);
                DataNode status = notificationData.GetNode("state");
                DataNode statusData = status.GetNodeByIndex(status.ChildCount - 1);
                RawTransaction resultTx = new RawTransaction();
                resultTx.TxID = txId;
                resultTx.Contract = lastExecution.GetString("contract");
                resultTx.EventType = HexToString(statusData.GetNodeByIndex(0).GetString("value"));
                resultTx.From = new UInt160(statusData.GetNodeByIndex(1).GetString("value").HexToBytes()).ToAddress();
                resultTx.To = new UInt160(statusData.GetNodeByIndex(2).GetString("value").HexToBytes()).ToAddress();
                String amountValue = statusData.GetNodeByIndex(3).GetString("value");
                bool isAmountArray = statusData.GetNodeByIndex(3).GetString("type").Equals("ByteArray");
                resultTx.Amount = isAmountArray? new BigInteger(amountValue.HexToBytes()): Int32.Parse(amountValue);
                if (findBlock)
                {
                    resultTx.Block = GetTransactionBlock(txId.Substring(2));
                }

                return resultTx;
            }

            return null;
        }


        public uint GetTransactionBlock(String txId)
        {
            DataNode rawApplication = RPC.QueryRPC("getrawtransaction", new Object[] {txId, 1});
            if (rawApplication != null)
            {
                DataNode resultNode = rawApplication.GetNode("result");
                String blockhash = resultNode.GetString("Blockhash");
                Block block = RPC.GetBlock(UInt256.Parse(blockhash));
                return block.Height;
            }

            return 0;
        }

        private string HexToString(string hex)
        {
            if (hex.Length % 2 != 0)
            {
                throw new ArgumentException();
            }

            var output = "";
            for (int i = 0; i <= hex.Length - 2; i += 2)
            {
                try
                {
                    var result = Convert.ToByte(new string(hex.Skip(i).Take(2).ToArray()), 16);
                    output += (Convert.ToChar(result));
                }
                catch (Exception)
                {
                    throw;
                }
            }

            return output;
        }
    }
}