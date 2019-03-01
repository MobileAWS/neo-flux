using System;
using System.Linq;
using System.Numerics;
using LunarLabs.Parser;
using Neo.Lux.Core;
using Neo.Lux.Cryptography;
using Neo.Lux.Utils;
using NeoFlux.Model;
using Org.BouncyCastle.Asn1.Mozilla;

namespace NeoFlux.Support
{
    public class LuxApiExtensions
    {
        private NeoRPC RPC { get; }

        public LuxApiExtensions(NeoRPC neoRpc)
        {
            RPC = neoRpc;
        }

        public RawTransaction GetTransferTransaction(String txId, bool findBlock)
        {
            var transactionLog = RPC.QueryRPC("getapplicationlog", new object[] {txId});
            if (transactionLog == null) return null;
            var resultNode = transactionLog.GetNode("result");
            var executions = resultNode.GetNode("executions");
            var lastExecution = executions.GetNodeByIndex(executions.ChildCount - 1);
            var notifications = lastExecution.GetNode("notifications");
            for (var i = 0; i < notifications.ChildCount; i++)
            {
                var resultTx = GetRawTransferTransaction(notifications.GetNodeByIndex(i), txId, findBlock);
                if (resultTx != null)
                {
                    return resultTx;
                }
            }
            return new RawTransaction();
        }

        private RawTransaction GetRawTransferTransaction(DataNode notificationData, string txId, bool findBlock)
        {            
            var status = notificationData.GetNode("state");
            var statusData = status.GetNodeByIndex(status.ChildCount - 1);
            var eventType = HexToString(statusData.GetNodeByIndex(0).GetString("value"));
            
            /** Only get transfer transactions */
            if (!eventType.Equals("transfer"))
            {
                return null;                
            }

            var resultTx = new RawTransaction
            {
                TxID = txId,
                Contract = notificationData.GetString("contract"),
                EventType = HexToString(statusData.GetNodeByIndex(0).GetString("value")),
                From = new UInt160(statusData.GetNodeByIndex(1).GetString("value").HexToBytes()).ToAddress(),
                To = new UInt160(statusData.GetNodeByIndex(2).GetString("value").HexToBytes()).ToAddress()
            };
            
            var amountValue = statusData.GetNodeByIndex(3).GetString("value");
            var isAmountArray = statusData.GetNodeByIndex(3).GetString("type").Equals("ByteArray");
            resultTx.Amount =
                isAmountArray ? new BigInteger(amountValue.HexToBytes()) : BigInteger.Parse(amountValue);
            if (findBlock)
            {
                resultTx.Block = GetTransactionBlock(txId.Substring(2));
            }

            return resultTx;

        }

        private uint GetTransactionBlock(String txId)
        {
            var rawApplication = RPC.QueryRPC("getrawtransaction", new Object[] {txId, 1});
            if (rawApplication == null) return 0;
            var resultNode = rawApplication.GetNode("result");
            var blockhash = resultNode.GetString("Blockhash");
            var block = RPC.GetBlock(UInt256.Parse(blockhash));
            return block.Height;

        }

        private static string HexToString(string hex)
        {
            if (hex.Length % 2 != 0)
            {
                throw new ArgumentException();
            }

            var output = "";
            for (var i = 0; i <= hex.Length - 2; i += 2)
            {
                var result = Convert.ToByte(new string(hex.Skip(i).Take(2).ToArray()), 16);
                output += (Convert.ToChar(result));
            }

            return output;
        }
    }
}
