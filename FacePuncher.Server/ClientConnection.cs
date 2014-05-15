﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;

using FacePuncher.Entities;
using FacePuncher.Geometry;
using FacePuncher.Network;
using System.Threading.Tasks;

namespace FacePuncher
{
    /// <summary>
    /// Handles communication with an individual client.
    /// </summary>
    class ClientConnection : Connection
    {
        /// <summary>
        /// Maximum range of a clients visibility.
        /// 
        /// TODO: Move to become a property of the player character.
        /// </summary>
        const int MaxVisibilityRange = 12;

        private TcpClient _socket;
        private RoomVisibility[] _visibility;

        /// <summary>
        /// The client's player entity.
        /// </summary>
        public Entity Player { get; private set; }

        /// <summary>
        /// Creates a new ClientConnection instance using a socket,
        /// additionally creating a player entity for the client in
        /// the specified level.
        /// </summary>
        /// <param name="socket">Socket connected to the client.</param>
        /// <param name="level">Level to create a player in.</param>
        public ClientConnection(TcpClient socket, Level level)
            : base(socket)
        {
            _socket = socket;

            _visibility = level
                .Select(x => new RoomVisibility(x))
                .ToArray();

            Player = Entity.Create("player");
            Player.GetComponent<PlayerControl>().Client = this;

            var rooms = level
                .Where(x => x.Any(y => y.State == TileState.Floor))
                .ToArray();

            var room = rooms[(int)(DateTime.Now.Ticks % rooms.Length)];

            var tiles = room
                .Where(x => x.State == TileState.Floor)
                .ToArray();

            Player.Place(tiles[Tools.Random.Next(tiles.Length)]);
        }

        /// <summary>
        /// Sends a single entity to the client.
        /// </summary>
        /// <param name="writer">Writer to write the entity to.</param>
        /// <param name="ent">Entity to send to the client.</param>
        private void SendEntity(Entity ent)
        {
            _stream.Write(ent.ID);
            _stream.Write(ent.ClassName);

            // TODO: Send component information?
        }

        /// <summary>
        /// Sends a partially observable level state update to the client.
        /// </summary>
        /// <param name="level">Level to send.</param>
        /// <param name="time">Current game time.</param>
        public void SendVisibleLevelState(Level level, ulong time)
        {
            _stream.Write((byte)0); // Pushed packet
            _stream.Write((byte)ServerPacketType.LevelState);

            _stream.Write(time);
            _stream.Write(Player.ID);

            lock (level)
            {
                var visibleRooms = _visibility
                    .Where(x => x.UpdateVisibility(Player.Position, MaxVisibilityRange, time))
                    .ToArray();

                _stream.Write(visibleRooms.Length);
                foreach (var vis in visibleRooms)
                {
                    _stream.Write(vis.Room.Rect);

                    var visibleTiles = vis.GetVisible(time).ToArray();

                    _stream.Write(visibleTiles.Length);
                    foreach (var tile in visibleTiles)
                    {
                        _stream.Write(tile.RelativePosition);
                        _stream.Write((byte)tile.State);

                        _stream.Write((ushort)tile.EntityCount);
                        foreach (var ent in tile)
                        {
                            SendEntity(ent);
                        }
                    }
                }
            }

            _stream.Flush();
        }

        protected override async Task HandlePushedPacket()
        {
            switch ((ClientPacketType)await _stream.ReadByteAsync())
            {
                case ClientPacketType.PlayerIntent:
                    Player.GetComponent<PlayerControl>().Intent = await _stream.ReadProtoBuf<Intent>();
                    break;
                default:
                    break;
            }
        }
    }
}
