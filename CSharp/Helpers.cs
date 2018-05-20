﻿using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using IncrementalMeanVarianceAccumulator;

namespace SortAlgoBench {
    static class Helpers {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Swap<T>(this T[] arr, int a, int b) { (arr[a], arr[b]) = (arr[b], arr[a]); }

        public static int ProcScale() {
            var splitIters = 4;
            var threads = Environment.ProcessorCount;
            while (threads > 0) {
                threads = threads >> 1;
                splitIters++;
            }

            return splitIters;
        }

        public static string MSE(MeanVarianceAccumulator acc)
            => MSE(acc.Mean, StdErr(acc));

        public static double CostScalingEstimate(double len)
            => len * Math.Sqrt(len + 40.0) + 15.0;

        public static double StdErr(MeanVarianceAccumulator acc)
            => acc.SampleStandardDeviation / Math.Sqrt(acc.WeightSum);

        public static string MSE(double mean, double stderr) {
            var significantDigits = Math.Log10(Math.Abs(mean / stderr));
            var digitsToShow = Math.Max(2, (int)(significantDigits + 2.5));
            
            if(Math.Pow(10,digitsToShow) <= mean && Math.Pow(10,digitsToShow+2) > mean)
                return mean.ToString("f0")+ "~" + stderr.ToString("f0");
            var fmtString =  "g" + digitsToShow;
            return mean.ToString(fmtString) + "~" + stderr.ToString("g2");
        }

        public static ulong[] RandomizeUInt64(int size) {
            var arr = new ulong[size];
            var r = new Random(37);
            for (var j = 0; j < arr.Length; j++)
                arr[j] = (((ulong)(uint)r.Next() << 32) + (uint)r.Next());
            return arr;
        }

        public static (int, long, DateTime, string, Guid) MapToBigStruct(ulong data) => ((int)(data >> 48), (long)(data - (data >> 48 << 48)), new DateTime(2000, 1, 1) + TimeSpan.FromSeconds((int)data), "test", default(Guid));
        public static (int, int, int) MapToSmallStruct(ulong data) => ((int)(data >> 32), (int)(data - (data >> 32 << 32)), (int)(data * 13));
        public static int MapToInt32(ulong data) => (int)(data >> 32);
        public static ulong MapToUInt64(ulong data) => data;
        public static uint MapToUInt32(ulong data) => (uint)(data >> 32);
        public static SampleClass MapToSampleClass(ulong data) => new SampleClass { Value = (int)(data >> 32) };
        public static double MapToDouble(ulong data) => (long)data / (double)(1L << 31);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool NeedsSort_WithBoundsCheck<T>(T[] array, int firstIdx, int endIdx) {
            if ((uint)firstIdx > (uint)endIdx || (uint)endIdx > (uint)array.Length) {
                ThrowIndexOutOfRange(array, firstIdx, endIdx);
            }
            return endIdx - firstIdx > 1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool NeedsSort_WithBoundsCheck<T>(T[] array, int endIdx) {
            if ((uint)endIdx > (uint)array.Length) {
                ThrowIndexOutOfRange(array, 0, endIdx);
            }
            return endIdx > 1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool NeedsSort_WithBoundsCheck<T>(T[] array) {
            return array.Length > 1;
        }

        public static void Sort<TOrder, T>(this TOrder order, T[] arr)
            where TOrder : struct, IOrdering<T>
            => OrderedAlgorithms<T, TOrder>.ParallelQuickSort(arr);

        public static void FastSort<T, TOrder>(this T[] arr, TOrder order)
            where TOrder : struct, IOrdering<T>
            => OrderedAlgorithms<T, TOrder>.ParallelQuickSort(arr);

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void ThrowIndexOutOfRange<T>(T[] array, int firstIdx, int lastIdx) {
            throw new IndexOutOfRangeException($"Attempted to sort [{firstIdx}, {lastIdx}), which not entirely within bounds of [0, {array.Length})");
        }

        public static IComparer<T> ComparerFor<T, TOrder>()
            where TOrder : IOrdering<T>
            => OrderComparer<T, TOrder>.Instance;

        class OrderComparer<T, TOrder> : IComparer<T>
            where TOrder : IOrdering<T> {
            public static IComparer<T> Instance = new OrderComparer<T, TOrder>();
            public int Compare(T x, T y)
                => default(TOrder).LessThan(x, y) ? -1
                : default(TOrder).LessThan(y, x) ? 1
                : 0;
        }
    }
}