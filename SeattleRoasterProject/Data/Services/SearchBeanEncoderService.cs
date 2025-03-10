﻿using System.Collections;
using System.Globalization;
using System.Text;
using RoasterBeansDataAccess.DataAccess;
using SeattleRoasterProject.Core.Enums;

namespace SeattleRoasterProject.Data.Services;

public class SearchBeanEncoderService
{
    // 0 Has grinder	1=true 0=false

    // Methods
    // 1 Pour-over		1=selected 0=unselected
    // 2 Immersion		1=selected 0=unselected
    // 3 Epresso		1=selected 0=unselected
    // 4 Cold Brew		1=selected 0=unselected
    // 5 Moka Pot		1=selected 0=unselected
    // 6 Drip			1=selected 0=unselected
    // 7 Anything		1=selected 0=unselected

    // Roasts
    // 8 Light			1=selected 0=unselected
    // 9 Medium			1=selected 0=unselected
    // 10 Dark			1=selected 0=unselected
    // 11 No Pref		1=selected 0=unselected

    // Origins
    // 12 Single-origin	1=selected 0=unselected
    // 13 Blend			1=selected 0=unselected
    // 14 No Pref		1=selected 0=unselected

    public string EncodeQuizResult(QuizSearchQuery query)
    {
        // Size must be multiple of six for encoding
        var encodedBits = new BitArray(16);
        encodedBits[0] = query.HasGrinder;

        encodedBits[8] = query.RoastAnswers[RoastLevel.Light];
        encodedBits[9] = query.RoastAnswers[RoastLevel.Medium];
        encodedBits[10] = query.RoastAnswers[RoastLevel.Dark];
        encodedBits[11] = query.AnyRoastLevelSelected;

        encodedBits[12] = query.SingleOriginSelected;
        encodedBits[13] = query.BlendSelected;
        encodedBits[14] = query.AnyOriginSelected;

        return ConvertBitArrayToString(encodedBits);
    }

    public BeanFilter DecodeQuizResult(string encodedResult)
    {
        var filter = new BeanFilter
        {
            IsExcluded = new FilterValueBool(true, false),
            IsInStock = new FilterValueBool(true, true)
        };

        var bitArray = ConvertHexToBitArray(encodedResult);

        // Grinder
        if (!bitArray[0])
        {
            filter.AvailablePreground = new FilterValueBool(true, true);
        }

        // Roast Levels
        if (!bitArray[11])
        {
            List<RoastLevel> levels = new();

            if (bitArray[8])
            {
                levels.Add(RoastLevel.Light);
            }

            if (bitArray[9])
            {
                levels.Add(RoastLevel.Medium);
            }

            if (bitArray[10])
            {
                levels.Add(RoastLevel.Dark);
            }

            filter.RoastFilter = new FilterList<RoastLevel>(true, levels);
        }

        // Origins
        if (!bitArray[14])
        {
            if (bitArray[12])
            {
                filter.IsSingleOrigin = new FilterValueBool(true, true);
            }
            else
            {
                filter.IsSingleOrigin = new FilterValueBool(true, false);
            }
        }

        return filter;
    }

    private string ConvertBitArrayToString(BitArray bitArray)
    {
        var sb = new StringBuilder(bitArray.Length / 4);

        for (var i = 0; i < bitArray.Length; i += 4)
        {
            var v = (bitArray[i] ? 8 : 0) |
                    (bitArray[i + 1] ? 4 : 0) |
                    (bitArray[i + 2] ? 2 : 0) |
                    (bitArray[i + 3] ? 1 : 0);

            sb.Append(v.ToString("x1")); // Or "X1"
        }

        return sb.ToString();
    }

    public static BitArray ConvertHexToBitArray(string hexData)
    {
        var ba = new BitArray(4 * hexData.Length);
        for (var i = 0; i < hexData.Length; i++)
        {
            var b = byte.Parse(hexData[i].ToString(), NumberStyles.HexNumber);
            for (var j = 0; j < 4; j++)
            {
                ba.Set(i * 4 + j, (b & (1 << (3 - j))) != 0);
            }
        }

        return ba;
    }
}

public class QuizSearchQuery
{
    public bool HasGrinder { get; set; }
    public Dictionary<RoastLevel, bool> RoastAnswers { get; set; } = new();
    public bool AnyRoastLevelSelected { get; set; }
    public bool SingleOriginSelected { get; set; }
    public bool BlendSelected { get; set; }
    public bool AnyOriginSelected { get; set; }
}