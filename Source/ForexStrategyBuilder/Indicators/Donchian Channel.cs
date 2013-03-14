// Donchian Channel Indicator
// Last changed on 2009-12-15
// Part of Forex Strategy Builder & Forex Strategy Trader
// Website http://forexsb.com/
// Copyright (c) 2006 - 2011 Miroslav Popov - All rights reserved.
// This code or any part of it cannot be used in other applications without a permission.

using System.Drawing;

namespace Forex_Strategy_Builder
{
    /// <summary>
    /// Donchian Channel Indicator
    /// </summary>
    public class Donchian_Channel : Indicator
    {
        /// <summary>
        /// Sets the default indicator parameters for the designated slot type
        /// </summary>
        public Donchian_Channel(SlotTypes slotType)
        {
            // General properties
            IndicatorName = "Donchian Channel";
            PossibleSlots = SlotTypes.Open | SlotTypes.OpenFilter | SlotTypes.Close | SlotTypes.CloseFilter;

            // Setting up the indicator parameters
            IndParam = new IndicatorParam();
            IndParam.IndicatorName = IndicatorName;
            IndParam.SlotType      = slotType;

            // The ComboBox parameters
            IndParam.ListParam[0].Caption = "Logic";
            if (slotType == SlotTypes.Open)
                IndParam.ListParam[0].ItemList = new string[]
                {
                    "Enter long at the Upper Band",
                    "Enter long at the Lower Band"
                };
            else if (slotType == SlotTypes.OpenFilter)
                IndParam.ListParam[0].ItemList = new string[]
                {
                    "The bar opens below the Upper Band",
                    "The bar opens above the Upper Band",
                    "The bar opens below the Lower Band",
                    "The bar opens above the Lower Band",
                    "The position opens below the Upper Band",
                    "The position opens above the Upper Band",
                    "The position opens below the Lower Band",
                    "The position opens above the Lower Band",
                    "The bar opens below the Upper Band after opening above it",
                    "The bar opens above the Upper Band after opening below it",
                    "The bar opens below the Lower Band after opening above it",
                    "The bar opens above the Lower Band after opening below it",
                };
            else if (slotType == SlotTypes.Close)
                IndParam.ListParam[0].ItemList = new string[]
                {
                    "Exit long at the Upper Band",
                    "Exit long at the Lower Band"
                };
            else if (slotType == SlotTypes.CloseFilter)
                IndParam.ListParam[0].ItemList = new string[]
                {
                    "The bar closes below the Upper Band",
                    "The bar closes above the Upper Band",
                    "The bar closes below the Lower Band",
                    "The bar closes above the Lower Band"
                };
            else
                IndParam.ListParam[0].ItemList = new string[]
                {
                    "Not Defined"
                };
            IndParam.ListParam[0].Index = 0;
            IndParam.ListParam[0].Text    = IndParam.ListParam[0].ItemList[IndParam.ListParam[0].Index];
            IndParam.ListParam[0].Enabled = true;
            IndParam.ListParam[0].ToolTip = "Logic of application of the Donchian Channel.";

            IndParam.ListParam[1].Caption  = "Base price";
            IndParam.ListParam[1].ItemList = new string[] { "High & Low" };
            IndParam.ListParam[1].Index    = 0;
            IndParam.ListParam[1].Text     = IndParam.ListParam[1].ItemList[IndParam.ListParam[1].Index];
            IndParam.ListParam[1].Enabled  = true;

            // The NumericUpDown parameters
            IndParam.NumParam[0].Caption = "Period";
            IndParam.NumParam[0].Value   = 10;
            IndParam.NumParam[0].Min     = 1;
            IndParam.NumParam[0].Max     = 200;
            IndParam.NumParam[0].Enabled = true;
            IndParam.NumParam[0].ToolTip = "The width of the range we are looking for an extreme in.";

            IndParam.NumParam[1].Caption = "Shift";
            IndParam.NumParam[1].Value   = 0;
            IndParam.NumParam[1].Min     = 0;
            IndParam.NumParam[1].Max     = 200;
            IndParam.NumParam[1].Enabled = true;
            IndParam.NumParam[1].ToolTip = "The number of bars to shift with.";

            // The CheckBox parameters
            IndParam.CheckParam[0].Caption = "Use previous bar value";
            IndParam.CheckParam[0].Checked = PrepareUsePrevBarValueCheckBox(slotType);
            IndParam.CheckParam[0].Enabled = true;
            IndParam.CheckParam[0].ToolTip = "Use the indicator value from the previous bar.";

            return;
        }

        /// <summary>
        /// Calculates the indicator's components
        /// </summary>
        public override void Calculate(SlotTypes slotType)
        {
            // Reading the parameters
            int iPeriod = (int)IndParam.NumParam[0].Value;
            int iShift  = (int)IndParam.NumParam[1].Value;
            int iPrvs   = IndParam.CheckParam[0].Checked ? 1 : 0;

            // Calculation
            double[] adUpBand  = new double[Bars];
            double[] adDnBand  = new double[Bars];

            int iFirstBar = iPeriod + iShift + iPrvs + 2;

            for (int iBar = iFirstBar; iBar < Bars - iShift; iBar++)
            {
                double dMax = double.MinValue;
                double dMin = double.MaxValue;
                for (int i = 0; i < iPeriod; i++)
                {
                    if (High[iBar - i] > dMax) dMax = High[iBar - i];
                    if (Low[iBar - i]  < dMin) dMin = Low[iBar - i];
                }
                adUpBand[iBar + iShift] = dMax;
                adDnBand[iBar + iShift] = dMin;
            }

            // Saving the components
            Component = new IndicatorComp[4];

            Component[0] = new IndicatorComp();
            Component[0].CompName   = "Upper Band";
            Component[0].DataType   = IndComponentType.IndicatorValue;
            Component[0].ChartType  = IndChartType.Line;
            Component[0].ChartColor = Color.Blue;
            Component[0].FirstBar   = iFirstBar;
            Component[0].Value      = adUpBand;

            Component[1] = new IndicatorComp();
            Component[1].CompName   = "Lower Band";
            Component[1].DataType   = IndComponentType.IndicatorValue;
            Component[1].ChartType  = IndChartType.Line;
            Component[1].ChartColor = Color.Blue;
            Component[1].FirstBar   = iFirstBar;
            Component[1].Value      = adDnBand;

            Component[2] = new IndicatorComp();
            Component[2].ChartType  = IndChartType.NoChart;
            Component[2].FirstBar   = iFirstBar;
            Component[2].Value      = new double[Bars];

            Component[3] = new IndicatorComp();
            Component[3].ChartType  = IndChartType.NoChart;
            Component[3].FirstBar   = iFirstBar;
            Component[3].Value      = new double[Bars];

            // Sets the Component's type.
            if (slotType == SlotTypes.Open)
            {
                Component[2].DataType = IndComponentType.OpenLongPrice;
                Component[2].CompName = "Long position entry price";
                Component[3].DataType = IndComponentType.OpenShortPrice;
                Component[3].CompName = "Short position entry price";
            }
            else if (slotType == SlotTypes.OpenFilter)
            {
                Component[2].DataType = IndComponentType.AllowOpenLong;
                Component[2].CompName = "Is long entry allowed";
                Component[3].DataType = IndComponentType.AllowOpenShort;
                Component[3].CompName = "Is short entry allowed";
            }
            else if (slotType == SlotTypes.Close)
            {
                Component[2].DataType = IndComponentType.CloseLongPrice;
                Component[2].CompName = "Long position closing price";
                Component[3].DataType = IndComponentType.CloseShortPrice;
                Component[3].CompName = "Short position closing price";
            }
            else if (slotType == SlotTypes.CloseFilter)
            {
                Component[2].DataType = IndComponentType.ForceCloseLong;
                Component[2].CompName = "Close out long position";
                Component[3].DataType = IndComponentType.ForceCloseShort;
                Component[3].CompName = "Close out short position";
            }

            if (slotType == SlotTypes.Open || slotType == SlotTypes.Close)
            {
                if (iPeriod > 1)
                {
                    for (int iBar = iFirstBar; iBar < Bars; iBar++)
                    {   // Covers the cases when the price can pass through the band without a signal.
                        double dOpen = Open[iBar]; // Current open price

                        // Upper band
                        double dValueUp   = adUpBand[iBar - iPrvs];     // Current value
                        double dValueUp1  = adUpBand[iBar - iPrvs - 1]; // Previous value
                        double dTempValUp = dValueUp;

                        if ((dValueUp1 > High[iBar - 1] && dValueUp < dOpen) || // The Open price jumps above the indicator
                            (dValueUp1 < Low[iBar -  1] && dValueUp > dOpen) || // The Open price jumps below the indicator
                            (Close[iBar - 1] < dValueUp && dValueUp < dOpen) || // The Open price is in a positive gap
                            (Close[iBar - 1] > dValueUp && dValueUp > dOpen))   // The Open price is in a negative gap
                            dTempValUp = dOpen; // The entry/exit level is moved to Open price

                        // Lower band
                        double dValueDown   = adDnBand[iBar - iPrvs];     // Current value
                        double dValueDown1  = adDnBand[iBar - iPrvs - 1]; // Previous value
                        double dTempValDown = dValueDown;

                        if ((dValueDown1 > High[iBar - 1] && dValueDown < dOpen) || // The Open price jumps above the indicator
                            (dValueDown1 < Low[iBar  - 1] && dValueDown > dOpen) || // The Open price jumps below the indicator
                            (Close[iBar - 1] < dValueDown && dValueDown < dOpen) || // The Open price is in a positive gap
                            (Close[iBar - 1] > dValueDown && dValueDown > dOpen))   // The Open price is in a negative gap
                            dTempValDown = dOpen; // The entry/exit level is moved to Open price

                        if (IndParam.ListParam[0].Text == "Enter long at the Upper Band" ||
                            IndParam.ListParam[0].Text == "Exit long at the Upper Band")
                        {
                            Component[2].Value[iBar] = dTempValUp;
                            Component[3].Value[iBar] = dTempValDown;
                        }
                        else
                        {
                            Component[2].Value[iBar] = dTempValDown;
                            Component[3].Value[iBar] = dTempValUp;
                        }
                    }
                }
                else
                {
                    for (int iBar = 2; iBar < Bars; iBar++)
                    {
                        if (IndParam.ListParam[0].Text == "Enter long at the Upper Band" ||
                            IndParam.ListParam[0].Text == "Exit long at the Upper Band")
                        {
                            Component[2].Value[iBar] = adUpBand[iBar - iPrvs];
                            Component[3].Value[iBar] = adDnBand[iBar - iPrvs];
                        }
                        else
                        {
                            Component[2].Value[iBar] = adDnBand[iBar - iPrvs];
                            Component[3].Value[iBar] = adUpBand[iBar - iPrvs];
                        }
                    }
                }
            }
            else
            {
                switch (IndParam.ListParam[0].Text)
                {
                    case "The bar opens below the Upper Band":
                        BandIndicatorLogic(iFirstBar, iPrvs, adUpBand, adDnBand, ref Component[2], ref Component[3], BandIndLogic.The_bar_opens_below_the_Upper_Band);
                        break;

                    case "The bar opens above the Upper Band":
                        BandIndicatorLogic(iFirstBar, iPrvs, adUpBand, adDnBand, ref Component[2], ref Component[3], BandIndLogic.The_bar_opens_above_the_Upper_Band);
                        break;

                    case "The bar opens below the Lower Band":
                        BandIndicatorLogic(iFirstBar, iPrvs, adUpBand, adDnBand, ref Component[2], ref Component[3], BandIndLogic.The_bar_opens_below_the_Lower_Band);
                        break;

                    case "The bar opens above the Lower Band":
                        BandIndicatorLogic(iFirstBar, iPrvs, adUpBand, adDnBand, ref Component[2], ref Component[3], BandIndLogic.The_bar_opens_above_the_Lower_Band);
                        break;

                    case "The bar opens below the Upper Band after opening above it":
                        BandIndicatorLogic(iFirstBar, iPrvs, adUpBand, adDnBand, ref Component[2], ref Component[3], BandIndLogic.The_bar_opens_below_the_Upper_Band_after_opening_above_it);
                        break;

                    case "The bar opens above the Upper Band after opening below it":
                        BandIndicatorLogic(iFirstBar, iPrvs, adUpBand, adDnBand, ref Component[2], ref Component[3], BandIndLogic.The_bar_opens_above_the_Upper_Band_after_opening_below_it);
                        break;

                    case "The bar opens below the Lower Band after opening above it":
                        BandIndicatorLogic(iFirstBar, iPrvs, adUpBand, adDnBand, ref Component[2], ref Component[3], BandIndLogic.The_bar_opens_below_the_Lower_Band_after_opening_above_it);
                        break;

                    case "The bar opens above the Lower Band after opening below it":
                        BandIndicatorLogic(iFirstBar, iPrvs, adUpBand, adDnBand, ref Component[2], ref Component[3], BandIndLogic.The_bar_opens_above_the_Lower_Band_after_opening_below_it);
                        break;

                    case "The position opens above the Upper Band":
                        Component[0].PosPriceDependence = PositionPriceDependence.PriceBuyHigher;
                        Component[1].PosPriceDependence = PositionPriceDependence.PriceSellLower;
                        Component[0].UsePreviousBar = iPrvs;
                        Component[1].UsePreviousBar = iPrvs;
                        Component[2].DataType = IndComponentType.Other;
                        Component[3].DataType = IndComponentType.Other;
                        Component[2].ShowInDynInfo = false;
                        Component[3].ShowInDynInfo = false;
                        break;

                    case "The position opens below the Upper Band":
                        Component[0].PosPriceDependence = PositionPriceDependence.PriceBuyLower;
                        Component[1].PosPriceDependence = PositionPriceDependence.PriceSellHigher;
                        Component[0].UsePreviousBar = iPrvs;
                        Component[1].UsePreviousBar = iPrvs;
                        Component[2].DataType = IndComponentType.Other;
                        Component[3].DataType = IndComponentType.Other;
                        Component[2].ShowInDynInfo = false;
                        Component[3].ShowInDynInfo = false;
                        break;

                    case "The position opens above the Lower Band":
                        Component[0].PosPriceDependence = PositionPriceDependence.PriceSellLower;
                        Component[1].PosPriceDependence = PositionPriceDependence.PriceBuyHigher;
                        Component[0].UsePreviousBar = iPrvs;
                        Component[1].UsePreviousBar = iPrvs;
                        Component[2].DataType = IndComponentType.Other;
                        Component[3].DataType = IndComponentType.Other;
                        Component[2].ShowInDynInfo = false;
                        Component[3].ShowInDynInfo = false;
                        break;

                    case "The position opens below the Lower Band":
                        Component[0].PosPriceDependence = PositionPriceDependence.PriceSellHigher;
                        Component[1].PosPriceDependence = PositionPriceDependence.PriceBuyLower;
                        Component[0].UsePreviousBar = iPrvs;
                        Component[1].UsePreviousBar = iPrvs;
                        Component[2].DataType = IndComponentType.Other;
                        Component[3].DataType = IndComponentType.Other;
                        Component[2].ShowInDynInfo = false;
                        Component[3].ShowInDynInfo = false;
                        break;

                    case "The bar closes below the Upper Band":
                        BandIndicatorLogic(iFirstBar, iPrvs, adUpBand, adDnBand, ref Component[2], ref Component[3], BandIndLogic.The_bar_closes_below_the_Upper_Band);
                        break;

                    case "The bar closes above the Upper Band":
                        BandIndicatorLogic(iFirstBar, iPrvs, adUpBand, adDnBand, ref Component[2], ref Component[3], BandIndLogic.The_bar_closes_above_the_Upper_Band);
                        break;

                    case "The bar closes below the Lower Band":
                        BandIndicatorLogic(iFirstBar, iPrvs, adUpBand, adDnBand, ref Component[2], ref Component[3], BandIndLogic.The_bar_closes_below_the_Lower_Band);
                        break;

                    case "The bar closes above the Lower Band":
                        BandIndicatorLogic(iFirstBar, iPrvs, adUpBand, adDnBand, ref Component[2], ref Component[3], BandIndLogic.The_bar_closes_above_the_Lower_Band);
                        break;

                    default:
                        break;
                }
            }

            return;
        }

        /// <summary>
        /// Sets the indicator logic description
        /// </summary>
        public override void SetDescription(SlotTypes slotType)
        {
            switch (IndParam.ListParam[0].Text)
            {
                case "Enter long at the Upper Band":
                    EntryPointLongDescription  = "at the Upper Band of " + ToString();
                    EntryPointShortDescription = "at the Lower Band of " + ToString();
                    break;

                case "Enter long at the Lower Band":
                    EntryPointLongDescription  = "at the Lower Band of " + ToString();
                    EntryPointShortDescription = "at the Upper Band of " + ToString();
                    break;

                case "Exit long at the Upper Band":
                    ExitPointLongDescription  = "at the Upper Band of " + ToString();
                    ExitPointShortDescription = "at the Lower Band of " + ToString();
                    break;

                case "Exit long at the Lower Band":
                    ExitPointLongDescription  = "at the Lower Band of " + ToString();
                    ExitPointShortDescription = "at the Upper Band of " + ToString();
                    break;

                case "The bar opens below the Upper Band":
                    EntryFilterLongDescription  = "the bar opens below the Upper Band of " + ToString();
                    EntryFilterShortDescription = "the bar opens above the Lower Band of " + ToString();
                    break;

                case "The bar opens above the Upper Band":
                    EntryFilterLongDescription  = "the bar opens above the Upper Band of " + ToString();
                    EntryFilterShortDescription = "the bar opens below the Lower Band of " + ToString();
                    break;

                case "The bar opens below the Lower Band":
                    EntryFilterLongDescription  = "the bar opens below the Lower Band of " + ToString();
                    EntryFilterShortDescription = "the bar opens above the Upper Band of " + ToString();
                    break;

                case "The bar opens above the Lower Band":
                    EntryFilterLongDescription  = "the bar opens above the Lower Band of " + ToString();
                    EntryFilterShortDescription = "the bar opens below the Upper Band of " + ToString();
                    break;

                case "The position opens above the Upper Band":
                    EntryFilterLongDescription  = "the position opening price is higher than the Upper Band of " + ToString();
                    EntryFilterShortDescription = "the position opening price is lower than the Lower Band of "  + ToString();
                    break;

                case "The position opens below the Upper Band":
                    EntryFilterLongDescription  = "the position opening price is lower than the Upper Band of "  + ToString();
                    EntryFilterShortDescription = "the position opening price is higher than the Lower Band of " + ToString();
                    break;

                case "The position opens above the Lower Band":
                    EntryFilterLongDescription  = "the position opening price is higher than the Lower Band of " + ToString();
                    EntryFilterShortDescription = "the position opening price is lower than the Upper Band of "  + ToString();
                    break;

                case "The position opens below the Lower Band":
                    EntryFilterLongDescription  = "the position opening price is lower than the Lower Band of "  + ToString();
                    EntryFilterShortDescription = "the position opening price is higher than the Upper Band of " + ToString();
                    break;

                case "The bar opens below the Upper Band after opening above it":
                    EntryFilterLongDescription  = "the bar opens below the Upper Band of " + ToString() + " after the previous bar has opened above it";
                    EntryFilterShortDescription = "the bar opens above the Lower Band of " + ToString() + " after the previous bar has opened below it";
                    break;

                case "The bar opens above the Upper Band after opening below it":
                    EntryFilterLongDescription  = "the bar opens above the Upper Band of " + ToString() + " after the previous bar has opened below it";
                    EntryFilterShortDescription = "the bar opens below the Lower Band of " + ToString() + " after the previous bar has opened above it";
                    break;

                case "The bar opens below the Lower Band after opening above it":
                    EntryFilterLongDescription  = "the bar opens below the Lower Band of " + ToString() + " after the previous bar has opened above it";
                    EntryFilterShortDescription = "the bar opens above the Upper Band of " + ToString() + " after the previous bar has opened below it";
                    break;

                case "The bar opens above the Lower Band after opening below it":
                    EntryFilterLongDescription  = "the bar opens above the Lower Band of " + ToString() + " after the previous bar has opened below it";
                    EntryFilterShortDescription = "the bar opens below the Upper Band of " + ToString() + " after the previous bar has opened above it";
                    break;

                case "The bar closes below the Upper Band":
                    ExitFilterLongDescription  = "the bar closes below the Upper Band of " + ToString();
                    ExitFilterShortDescription = "the bar closes above the Lower Band of " + ToString();
                    break;

                case "The bar closes above the Upper Band":
                    ExitFilterLongDescription  = "the bar closes above the Upper Band of " + ToString();
                    ExitFilterShortDescription = "the bar closes below the Lower Band of " + ToString();
                    break;

                case "The bar closes below the Lower Band":
                    ExitFilterLongDescription  = "the bar closes below the Lower Band of " + ToString();
                    ExitFilterShortDescription = "the bar closes above the Upper Band of " + ToString();
                    break;

                case "The bar closes above the Lower Band":
                    ExitFilterLongDescription  = "the bar closes above the Lower Band of " + ToString();
                    ExitFilterShortDescription = "the bar closes below the Upper Band of " + ToString();
                    break;

                default:
                    break;
            }

            return;
        }

        /// <summary>
        /// Indicator to string
        /// </summary>
        public override string ToString()
        {
            string sString = IndicatorName +
                (IndParam.CheckParam[0].Checked ? "* (" : " (") +
                IndParam.NumParam[0].ValueToString + ", " + // Period
                IndParam.NumParam[1].ValueToString + ")";   // Shift

            return sString;
        }
    }
}
