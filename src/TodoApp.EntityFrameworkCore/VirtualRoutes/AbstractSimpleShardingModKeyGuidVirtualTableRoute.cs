﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using ShardingCore.Core.VirtualRoutes;
using ShardingCore.Core.VirtualRoutes.TableRoutes.Abstractions;
using ShardingCore.Helpers;

namespace TodoApp.VirtualRoutes
{
    public abstract class AbstractSimpleShardingModKeyGuidVirtualTableRoute<TEntity> : AbstractShardingOperatorVirtualTableRoute<TEntity, Guid> where TEntity : class
    {
        protected readonly int Mod;
        protected readonly int TailLength;
        protected readonly char PaddingChar;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="tailLength">猴子长度</param>
        /// <param name="mod">取模被除数</param>
        /// <param name="paddingChar">当取模后不足tailLength左补什么参数</param>
        protected AbstractSimpleShardingModKeyGuidVirtualTableRoute(int tailLength, int mod, char paddingChar = '0')
        {
            if (tailLength < 1)
                throw new ArgumentException($"{nameof(tailLength)} less than 1 ");
            if (mod < 1)
                throw new ArgumentException($"{nameof(mod)} less than 1 ");
            if (string.IsNullOrWhiteSpace(paddingChar.ToString()))
                throw new ArgumentException($"{nameof(paddingChar)} cant empty ");
            TailLength = tailLength;
            Mod = mod;
            PaddingChar = paddingChar;
        }
        /// <summary>
        /// 如何将shardingkey转成对应的tail
        /// </summary>
        /// <param name="shardingKey"></param>
        /// <returns></returns>
        public override string ShardingKeyToTail(object shardingKey)
        {
            var shardingKeyStr = shardingKey.ToString();
            return Math.Abs(ShardingCoreHelper.GetStringHashCode(shardingKeyStr) % Mod).ToString().PadLeft(TailLength, PaddingChar);
        }
        /// <summary>
        /// 获取对应类型在数据库中的所有后缀
        /// </summary>
        /// <returns></returns>
        public override List<string> GetAllTails()
        {
            return Enumerable.Range(0, Mod).Select(o => o.ToString().PadLeft(TailLength, PaddingChar)).ToList();
        }
        /// <summary>
        /// 路由表达式如何路由到正确的表
        /// </summary>
        /// <param name="shardingKey"></param>
        /// <param name="shardingOperator"></param>
        /// <returns></returns>
        public override Expression<Func<string, bool>> GetRouteToFilter(Guid shardingKey, ShardingOperatorEnum shardingOperator)
        {
            var t = ShardingKeyToTail(shardingKey);
            switch (shardingOperator)
            {
                case ShardingOperatorEnum.Equal: return tail => tail == t;
                default:
                    {
#if DEBUG
                        Console.WriteLine($"shardingOperator is not equal scan all table tail");
#endif
                        return tail => true;
                    }
            }
        }
    }
}