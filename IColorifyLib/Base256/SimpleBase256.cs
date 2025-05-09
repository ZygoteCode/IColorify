﻿using System.Text;

namespace SimpleEncoding
{
    class SimpleBase256 : SimpleBase
    {
        public const string DefaultAlphabet = "!#$%&'()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\\]^_`abcdefghijklmnopqrstuvwxyz{|}~ ¡¢£¤¥¦§¨©ª«¬­®¯°±²³´µ¶·¸¹º»¼½¾¿ÀÁÂÃÄÅÆÇÈÉÊËÌÍÎÏÐÑÒÓÔÕÖ×ØÙÚÛÜÝÞßàáâãäåæçèéêëìíîïðñòóôõö÷øùúûüýþÿĀāĂăĄąĆćĈĉĊċČčĎďĐđĒēĔĕĖėĘęĚěĜĝĞğĠġĢģĤĥĦħĨĩĪīĬĭĮįİıĲĳĴĵĶķĸĹĺĻļĽľĿŀŁł";
        public const char DefaultSpecial = (char)0;

        public override bool HaveSpecial
        {
            get { return false; }
        }

        public SimpleBase256(string alphabet = DefaultAlphabet, char special = DefaultSpecial,
            Encoding textEncoding = null)
            : base(256, alphabet, special, textEncoding)
        {
        }

        public override string Encode(byte[] data)
        {
            StringBuilder result = new StringBuilder(data.Length);

            for (int i = 0; i < data.Length; i++)
                result.Append(Alphabet[data[i]]);

            return result.ToString();
        }

        public override byte[] Decode(string data)
        {
            unchecked
            {
                byte[] result = new byte[data.Length];

                for (int i = 0; i < data.Length; i++)
                    result[i] = (byte)InvAlphabet[data[i]];

                return result;
            }
        }
    }
}