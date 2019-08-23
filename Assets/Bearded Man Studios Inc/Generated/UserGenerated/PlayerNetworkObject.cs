using BeardedManStudios.Forge.Networking.Frame;
using BeardedManStudios.Forge.Networking.Unity;
using System;
using UnityEngine;

namespace BeardedManStudios.Forge.Networking.Generated
{
	[GeneratedInterpol("{\"inter\":[0,0,0]")]
	public partial class PlayerNetworkObject : NetworkObject
	{
		public const int IDENTITY = 7;

		private byte[] _dirtyFields = new byte[1];

		#pragma warning disable 0067
		public event FieldChangedEvent fieldAltered;
		#pragma warning restore 0067
		[ForgeGeneratedField]
		private Vector2 _moveDirection;
		public event FieldEvent<Vector2> moveDirectionChanged;
		public InterpolateVector2 moveDirectionInterpolation = new InterpolateVector2() { LerpT = 0f, Enabled = false };
		public Vector2 moveDirection
		{
			get { return _moveDirection; }
			set
			{
				// Don't do anything if the value is the same
				if (_moveDirection == value)
					return;

				// Mark the field as dirty for the network to transmit
				_dirtyFields[0] |= 0x1;
				_moveDirection = value;
				hasDirtyFields = true;
			}
		}

		public void SetmoveDirectionDirty()
		{
			_dirtyFields[0] |= 0x1;
			hasDirtyFields = true;
		}

		private void RunChange_moveDirection(ulong timestep)
		{
			if (moveDirectionChanged != null) moveDirectionChanged(_moveDirection, timestep);
			if (fieldAltered != null) fieldAltered("moveDirection", _moveDirection, timestep);
		}
		[ForgeGeneratedField]
		private Vector2 _gridPosition;
		public event FieldEvent<Vector2> gridPositionChanged;
		public InterpolateVector2 gridPositionInterpolation = new InterpolateVector2() { LerpT = 0f, Enabled = false };
		public Vector2 gridPosition
		{
			get { return _gridPosition; }
			set
			{
				// Don't do anything if the value is the same
				if (_gridPosition == value)
					return;

				// Mark the field as dirty for the network to transmit
				_dirtyFields[0] |= 0x2;
				_gridPosition = value;
				hasDirtyFields = true;
			}
		}

		public void SetgridPositionDirty()
		{
			_dirtyFields[0] |= 0x2;
			hasDirtyFields = true;
		}

		private void RunChange_gridPosition(ulong timestep)
		{
			if (gridPositionChanged != null) gridPositionChanged(_gridPosition, timestep);
			if (fieldAltered != null) fieldAltered("gridPosition", _gridPosition, timestep);
		}
		[ForgeGeneratedField]
		private bool _isDead;
		public event FieldEvent<bool> isDeadChanged;
		public Interpolated<bool> isDeadInterpolation = new Interpolated<bool>() { LerpT = 0f, Enabled = false };
		public bool isDead
		{
			get { return _isDead; }
			set
			{
				// Don't do anything if the value is the same
				if (_isDead == value)
					return;

				// Mark the field as dirty for the network to transmit
				_dirtyFields[0] |= 0x4;
				_isDead = value;
				hasDirtyFields = true;
			}
		}

		public void SetisDeadDirty()
		{
			_dirtyFields[0] |= 0x4;
			hasDirtyFields = true;
		}

		private void RunChange_isDead(ulong timestep)
		{
			if (isDeadChanged != null) isDeadChanged(_isDead, timestep);
			if (fieldAltered != null) fieldAltered("isDead", _isDead, timestep);
		}

		protected override void OwnershipChanged()
		{
			base.OwnershipChanged();
			SnapInterpolations();
		}
		
		public void SnapInterpolations()
		{
			moveDirectionInterpolation.current = moveDirectionInterpolation.target;
			gridPositionInterpolation.current = gridPositionInterpolation.target;
			isDeadInterpolation.current = isDeadInterpolation.target;
		}

		public override int UniqueIdentity { get { return IDENTITY; } }

		protected override BMSByte WritePayload(BMSByte data)
		{
			UnityObjectMapper.Instance.MapBytes(data, _moveDirection);
			UnityObjectMapper.Instance.MapBytes(data, _gridPosition);
			UnityObjectMapper.Instance.MapBytes(data, _isDead);

			return data;
		}

		protected override void ReadPayload(BMSByte payload, ulong timestep)
		{
			_moveDirection = UnityObjectMapper.Instance.Map<Vector2>(payload);
			moveDirectionInterpolation.current = _moveDirection;
			moveDirectionInterpolation.target = _moveDirection;
			RunChange_moveDirection(timestep);
			_gridPosition = UnityObjectMapper.Instance.Map<Vector2>(payload);
			gridPositionInterpolation.current = _gridPosition;
			gridPositionInterpolation.target = _gridPosition;
			RunChange_gridPosition(timestep);
			_isDead = UnityObjectMapper.Instance.Map<bool>(payload);
			isDeadInterpolation.current = _isDead;
			isDeadInterpolation.target = _isDead;
			RunChange_isDead(timestep);
		}

		protected override BMSByte SerializeDirtyFields()
		{
			dirtyFieldsData.Clear();
			dirtyFieldsData.Append(_dirtyFields);

			if ((0x1 & _dirtyFields[0]) != 0)
				UnityObjectMapper.Instance.MapBytes(dirtyFieldsData, _moveDirection);
			if ((0x2 & _dirtyFields[0]) != 0)
				UnityObjectMapper.Instance.MapBytes(dirtyFieldsData, _gridPosition);
			if ((0x4 & _dirtyFields[0]) != 0)
				UnityObjectMapper.Instance.MapBytes(dirtyFieldsData, _isDead);

			// Reset all the dirty fields
			for (int i = 0; i < _dirtyFields.Length; i++)
				_dirtyFields[i] = 0;

			return dirtyFieldsData;
		}

		protected override void ReadDirtyFields(BMSByte data, ulong timestep)
		{
			if (readDirtyFlags == null)
				Initialize();

			Buffer.BlockCopy(data.byteArr, data.StartIndex(), readDirtyFlags, 0, readDirtyFlags.Length);
			data.MoveStartIndex(readDirtyFlags.Length);

			if ((0x1 & readDirtyFlags[0]) != 0)
			{
				if (moveDirectionInterpolation.Enabled)
				{
					moveDirectionInterpolation.target = UnityObjectMapper.Instance.Map<Vector2>(data);
					moveDirectionInterpolation.Timestep = timestep;
				}
				else
				{
					_moveDirection = UnityObjectMapper.Instance.Map<Vector2>(data);
					RunChange_moveDirection(timestep);
				}
			}
			if ((0x2 & readDirtyFlags[0]) != 0)
			{
				if (gridPositionInterpolation.Enabled)
				{
					gridPositionInterpolation.target = UnityObjectMapper.Instance.Map<Vector2>(data);
					gridPositionInterpolation.Timestep = timestep;
				}
				else
				{
					_gridPosition = UnityObjectMapper.Instance.Map<Vector2>(data);
					RunChange_gridPosition(timestep);
				}
			}
			if ((0x4 & readDirtyFlags[0]) != 0)
			{
				if (isDeadInterpolation.Enabled)
				{
					isDeadInterpolation.target = UnityObjectMapper.Instance.Map<bool>(data);
					isDeadInterpolation.Timestep = timestep;
				}
				else
				{
					_isDead = UnityObjectMapper.Instance.Map<bool>(data);
					RunChange_isDead(timestep);
				}
			}
		}

		public override void InterpolateUpdate()
		{
			if (IsOwner)
				return;

			if (moveDirectionInterpolation.Enabled && !moveDirectionInterpolation.current.UnityNear(moveDirectionInterpolation.target, 0.0015f))
			{
				_moveDirection = (Vector2)moveDirectionInterpolation.Interpolate();
				//RunChange_moveDirection(moveDirectionInterpolation.Timestep);
			}
			if (gridPositionInterpolation.Enabled && !gridPositionInterpolation.current.UnityNear(gridPositionInterpolation.target, 0.0015f))
			{
				_gridPosition = (Vector2)gridPositionInterpolation.Interpolate();
				//RunChange_gridPosition(gridPositionInterpolation.Timestep);
			}
			if (isDeadInterpolation.Enabled && !isDeadInterpolation.current.UnityNear(isDeadInterpolation.target, 0.0015f))
			{
				_isDead = (bool)isDeadInterpolation.Interpolate();
				//RunChange_isDead(isDeadInterpolation.Timestep);
			}
		}

		private void Initialize()
		{
			if (readDirtyFlags == null)
				readDirtyFlags = new byte[1];

		}

		public PlayerNetworkObject() : base() { Initialize(); }
		public PlayerNetworkObject(NetWorker networker, INetworkBehavior networkBehavior = null, int createCode = 0, byte[] metadata = null) : base(networker, networkBehavior, createCode, metadata) { Initialize(); }
		public PlayerNetworkObject(NetWorker networker, uint serverId, FrameStream frame) : base(networker, serverId, frame) { Initialize(); }

		// DO NOT TOUCH, THIS GETS GENERATED PLEASE EXTEND THIS CLASS IF YOU WISH TO HAVE CUSTOM CODE ADDITIONS
	}
}
