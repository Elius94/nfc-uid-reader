using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestMifare
{
    internal class Program
    {
        static NFC nfc = null;

        static void Main(string[] args)
        {
            nfc = new NFC();
            nfc.Init(0, false);
            nfc.CardUidReceived += OnCardUidReceivedSlot;

            while (true)
            {

            }
        }

        // Eseguita quando l'utente avvicina la card al lettore: restituisce solo l'UID
        private static void OnCardUidReceivedSlot(byte[] uid)
        {
            Console.WriteLine("UID: " + BitConverter.ToString(uid));
        }
    }
}
