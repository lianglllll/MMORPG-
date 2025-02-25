﻿using GameServer.Net;
using System;
using GameServer.Model;
using GameServer.Manager;
using Common.Summer.Tools;
using Common.Summer.Net;
using Common.Summer.Core;
using HS.Protobuf.Scene;
using HS.Protobuf.SceneEntity;

namespace GameServer.Service
{
    public class SpaceService:Singleton<SpaceService>
    {

        /// <summary>
        /// 开启服务
        /// </summary>
        public void Start()
        {
            //初始化全部地图
            SpaceManager.Instance.init();

            //订阅信息同步请求
            MessageRouter.Instance.Subscribe<SpaceEntitySyncRequest>(_SpaceEntitySyncRequest);
           
        }

        /// <summary>
        /// 根据spaceId获取地图
        /// </summary>
        /// <param name="spaceId"></param>
        /// <returns></returns>
        public Space GetSpaceById(int spaceId)
        {
            return SpaceManager.Instance.GetSpaceById(spaceId);          
        }

        /// <summary>
        /// 角色信息同步请求
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="msg"></param>
        private void _SpaceEntitySyncRequest(Connection conn, SpaceEntitySyncRequest msg)
        {
            //获取当前连接的场景对象
            Character chr = conn.Get<Session>().character;

            //不接受死亡角色的同步
            if (chr.IsDeath) return;

            //判断合理性
            NetEntity nEntity = msg.EntitySync.Entity;//请求位置信息
            //将要移动的距离
            float distance = Vector3Int.Distance(nEntity.Position, chr.Position);
            //计算时间差
            float timeDistance = Math.Min(chr.PositionUpdateTimeDistance, 1.0f);
            //计算距离限额
            float limit = chr.Speed * timeDistance * 1.5f;
            //裁决
            if (float.IsNaN(distance)||distance > limit)
            {
                //方案1：拉回原位置
                /*SpaceEntitySyncResponse resp = new SpaceEntitySyncResponse();
                resp.EntitySync = new NEntitySync();
                resp.EntitySync.Entity = serverEntity.EntityData;
                resp.EntitySync.Force = true;
                conn.Send(resp);*/

                //方案2：记录异常【玩家、角色、原位置、目标位置、时间差、当前时间】

            }

            //转发space处理
            chr.currentSpace.SyncActor(msg.EntitySync,chr);
        }

    }
}
