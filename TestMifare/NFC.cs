using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using MiFare;
using MiFare.Classic;
using MiFare.Devices;
using MiFare.PcSc;

namespace TestMifare
{
    public class NFC
    {
        private SmartCardReader reader;
        private MiFareCard card;
        private IReadOnlyList<string> readers;

        private bool verbose = false;

        public delegate void CardSectorReceivedEventHandler(int selectedSector, byte[] data);
        public event CardSectorReceivedEventHandler CardSectorReceived;

        public delegate void CardUidReceivedEventHandler(byte[] uid);
        public event CardUidReceivedEventHandler CardUidReceived;

        public int selectedSector = 0;

        public NFC()
        {
            readers = GetReaders();
        }

        public void Init(int deviceId, bool _verbose = false)
        {
            this.verbose = _verbose;
            GetDevices(deviceId);
        }

        public void SetSelectedSector(int _selectedSector)
        {
            this.selectedSector = _selectedSector;
        }

        private IReadOnlyList<string> GetReaders()
        {
            return CardReader.GetReaderNames();
        }

        private async void GetDevices(int deviceId)
        {
            try
            {
                reader = await CardReader.FindAsync(readers[deviceId]);
                if (reader == null)
                {
                    Log("No Readers Found");
                    return;
                }

                reader.CardAdded += CardAdded;
                reader.CardRemoved += CardRemoved;
            }
            catch (Exception e)
            {
                Log("Exception: " + e.Message);
            }
        }


        private void CardRemoved(object sender, EventArgs e)
        {
            Debug.WriteLine("Card Removed");
            card?.Dispose();
            card = null;

        }

        private async void CardAdded(object sender, CardEventArgs args)
        {
            Debug.WriteLine("Card Added");
            try
            {
                await HandleCard(args);
            }
            catch (Exception ex)
            {
                Log("CardAdded Exception: " + ex.Message);
            }
        }

        private async Task HandleCard(CardEventArgs args)
        {
            try
            {
                card?.Dispose();
                card = args.SmartCard.CreateMiFareCard();
                var localCard = card;
                var cardIdentification = await localCard.GetCardInfo();
                Log("Connected to card\r\nPC/SC device class: " + cardIdentification.PcscDeviceClass.ToString() + "\r\nCard name: " + cardIdentification.PcscCardName.ToString());

                if (cardIdentification.PcscDeviceClass == MiFare.PcSc.DeviceClass.StorageClass
                     && (cardIdentification.PcscCardName == CardName.MifareStandard1K || cardIdentification.PcscCardName == CardName.MifareStandard4K))
                {
                    try
                    {
                        var uid = await localCard.GetUid();
                        CardUidReceived?.Invoke(uid);
                        Log("UID:  " + BitConverter.ToString(uid));
                        var data = await localCard.GetData(selectedSector, 0, 48);
                        CardSectorReceived?.Invoke(selectedSector, data);
                    }
                    catch (Exception)
                    {
                        Log("Failed to load sector: " + selectedSector);
                    }

                }
            }
            catch (Exception e)
            {
                Log("HandleCard Exception: " + e.Message);
            }
        }

        private void Log(string message)
        {
            if (verbose)
            {
                Console.WriteLine(message);
            }
        }
    }
}
