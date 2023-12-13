
/** HitNew.cs
*
*	Created by LIAM WOFFORD of CUBEROOT SOFTWARE, LLC.
*
*	Free to use or modify, with or without creditation,
*	under the Creative Commons 0 License.
*/

#region Includes

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#endregion

namespace Mithril
{
	#region HitPoolBase

	public abstract class HitPoolBase : object
	{
		/// <summary>
		/// The total number of hits detected; this can never be larger than <see cref="bufferSize"/>.
		///</summary>
		private int _length = 0;
		/// <inheritdoc cref="_length"/>
		public int length { get => _length; protected set => _length = value.Min(bufferSize); }

		/// <summary>
		/// Whether or not any hits were detected.
		///</summary>
		public bool blocked => length != 0;

		/// <summary>
		/// Maximmum number of hits it's possible to detect.
		///</summary>
		public abstract int bufferSize { get; }
	}

	#endregion
	#region HitPool<T>

	public abstract class HitPool<T> : HitPoolBase, IEnumerable<T>, IEnumerator<T>
	where T : unmanaged
	{
		protected T[] hits { get; private set; }
		private int _iterator = 0;

		public sealed override int bufferSize => hits.Length;

		public HitPool(int bufferSize)
		{
			hits = new T[bufferSize];
		}

		public abstract T GetClosest();

		public T this[int i]
		{
			get
			{
				if (i >= length) throw new IndexOutOfRangeException();
				return hits[i];
			}
		}

		public IEnumerator<T> GetEnumerator() => this;
		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

		public T Current => hits[_iterator];
		object IEnumerator.Current => Current;

		public void Dispose() { /*/ Unused /*/ }
		public bool MoveNext()
		{
			_iterator++;
			return _iterator < length;
		}
		public void Reset() => _iterator = -1;
	}

	#endregion
	#region HitPool

	public sealed class HitPool : HitPool<RaycastHit>
	{
		public HitPool(int bufferSize) : base(bufferSize) { }

		public override RaycastHit GetClosest()
		{
			if (length == 0) throw new UnityException();
			if (length == 1) return this[0];

			float shortestDistance = float.MaxValue;
			int iResult = -1;
			for (var i = 0; i < length; i++)
			{
				if (this[i].distance > shortestDistance) continue;
				shortestDistance = this[i].distance;
				iResult = i;
			}
			return this[iResult];
		}

		#region LineCast

		public void LineCast(Vector3 origin, Vector3 direction, float maxDistance, LayerMask layerMask, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
		{
			length = Physics.RaycastNonAlloc(origin, direction, hits, maxDistance, layerMask, queryTriggerInteraction);
		}
		public void LineCast(Ray ray, float maxDistance, LayerMask layerMask, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
		{
			length = Physics.RaycastNonAlloc(ray, hits, maxDistance, layerMask, queryTriggerInteraction);
		}
		public void LineCast(Vector3 origin, Vector3 target, LayerMask layerMask, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
		{
			var delta = target - origin;
			length = Physics.RaycastNonAlloc(origin, delta.normalized, hits, delta.magnitude, layerMask, queryTriggerInteraction);
		}

		#endregion
		#region BoxCast

		public void BoxCast(Vector3 origin, Quaternion rotation, Vector3 halfExtents, Vector3 direction, float maxDistance, LayerMask layerMask, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
		{
			length = Physics.BoxCastNonAlloc(origin, halfExtents, direction, hits, rotation, maxDistance, layerMask, queryTriggerInteraction);
		}
		public void BoxCast(Vector3 origin, Quaternion rotation, Vector3 halfExtents, Vector3 target, LayerMask layerMask, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
		{
			var delta = target - origin;
			length = Physics.BoxCastNonAlloc(origin, halfExtents, delta.normalized, hits, rotation, delta.magnitude, layerMask, queryTriggerInteraction);
		}
		public void BoxCast(Vector3 origin, Vector3 halfExtents, Vector3 direction, float maxDistance, LayerMask layerMask, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
		{
			length = Physics.BoxCastNonAlloc(origin, halfExtents, direction, hits, Quaternion.identity, maxDistance, layerMask, queryTriggerInteraction);
		}
		public void BoxCast(Vector3 origin, Vector3 halfExtents, Vector3 target, LayerMask layerMask, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
		{
			var delta = target - origin;
			length = Physics.BoxCastNonAlloc(origin, halfExtents, delta.normalized, hits, Quaternion.identity, delta.magnitude, layerMask, queryTriggerInteraction);
		}
		public void BoxCast(Bounds bounds, Vector3 direction, float maxDistance, LayerMask layerMask, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
		{
			length = Physics.BoxCastNonAlloc(bounds.center, bounds.extents, direction, hits, Quaternion.identity, maxDistance, layerMask, QueryTriggerInteraction.UseGlobal);
		}
		public void BoxCast(Bounds bounds, Vector3 target, LayerMask layerMask, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
		{
			var delta = target - bounds.center;
			length = Physics.BoxCastNonAlloc(bounds.center, bounds.extents, delta.normalized, hits, Quaternion.identity, delta.magnitude, layerMask, QueryTriggerInteraction.UseGlobal);
		}
		public void BoxCast(BoxCollider box, Vector3 direction, float maxDistance, LayerMask layerMask, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
		{
			length = Physics.BoxCastNonAlloc(box.transform.position + box.center, box.size * 0.5f, direction, hits, box.transform.rotation, maxDistance, layerMask, queryTriggerInteraction);
		}
		public void BoxCast(BoxCollider box, Vector3 target, LayerMask layerMask, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
		{
			var origin = box.transform.position + box.center;
			var delta = target - origin;
			length = Physics.BoxCastNonAlloc(origin, box.size * 0.5f, delta.normalized, hits, box.transform.rotation, delta.magnitude, layerMask, queryTriggerInteraction);
		}
		public void BoxCast(Vector3 origin, BoxCollider box, Vector3 direction, float maxDistance, LayerMask layerMask, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
		{
			length = Physics.BoxCastNonAlloc(origin, box.size * 0.5f, direction, hits, box.transform.rotation, maxDistance, layerMask, queryTriggerInteraction);
		}
		public void BoxCast(Vector3 origin, BoxCollider box, Vector3 target, LayerMask layerMask, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
		{
			var delta = target - origin;
			length = Physics.BoxCastNonAlloc(origin, box.size * 0.5f, delta.normalized, hits, box.transform.rotation, delta.magnitude, layerMask, queryTriggerInteraction);
		}

		#endregion
		#region SphereCast

		public void SphereCast(Vector3 origin, float radius, Vector3 direction, float maxDistance, LayerMask layerMask, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
		{
			length = Physics.SphereCastNonAlloc(origin, radius, direction, hits, maxDistance, layerMask, queryTriggerInteraction);
		}
		public void SphereCast(Vector3 origin, float radius, Vector3 target, LayerMask layerMask, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
		{
			var delta = target - origin;
			length = Physics.SphereCastNonAlloc(origin, radius, delta.normalized, hits, delta.magnitude, layerMask, queryTriggerInteraction);
		}
		public void SphereCast(Bounds bounds, Vector3 direction, float maxDistance, LayerMask layerMask, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
		{
			length = Physics.SphereCastNonAlloc(bounds.center, bounds.extents.MaxComponent(), direction, hits, maxDistance, layerMask, queryTriggerInteraction);
		}
		public void SphereCast(Bounds bounds, Vector3 target, LayerMask layerMask, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
		{
			var delta = target - bounds.center;
			length = Physics.SphereCastNonAlloc(bounds.center, bounds.extents.MaxComponent(), delta.normalized, hits, delta.magnitude, layerMask, queryTriggerInteraction);
		}
		public void SphereCast(SphereCollider sphere, Vector3 direction, float maxDistance, LayerMask layerMask, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
		{
			length = Physics.SphereCastNonAlloc(sphere.transform.position + sphere.center, sphere.radius, direction, hits, maxDistance, layerMask, queryTriggerInteraction);
		}
		public void SphereCast(SphereCollider sphere, Vector3 target, LayerMask layerMask, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
		{
			var origin = sphere.transform.position + sphere.center;
			var delta = target - origin;
			length = Physics.SphereCastNonAlloc(origin, sphere.radius, delta.normalized, hits, delta.magnitude, layerMask, queryTriggerInteraction);
		}
		public void SphereCast(Vector3 origin, SphereCollider sphere, Vector3 direction, float maxDistance, LayerMask layerMask, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
		{
			length = Physics.SphereCastNonAlloc(origin, sphere.radius, direction, hits, maxDistance, layerMask, queryTriggerInteraction);
		}
		public void SphereCast(Vector3 origin, SphereCollider sphere, Vector3 target, LayerMask layerMask, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
		{
			var delta = target - origin;
			length = Physics.SphereCastNonAlloc(origin, sphere.radius, delta.normalized, hits, delta.magnitude, layerMask, queryTriggerInteraction);
		}

		#endregion
		#region CapsuleCast

		public void CapsuleCast(Vector3 point1, Vector3 point2, float radius, Vector3 direction, float maxDistance, LayerMask layerMask, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
		{
			length = Physics.CapsuleCastNonAlloc(point1, point2, radius, direction, hits, maxDistance, layerMask, queryTriggerInteraction);
		}
		public void CapsuleCast(Vector3 point1, Vector3 point2, float radius, Vector3 target, LayerMask layerMask, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
		{
			var midpoint = Math.Midpoint(point1, point2);
			var delta = target - midpoint;
			length = Physics.CapsuleCastNonAlloc(point1, point2, radius, delta.normalized, hits, delta.magnitude, layerMask, queryTriggerInteraction);
		}
		public void CapsuleCast(Vector3 origin, Quaternion rotation, float radius, float height, Vector3 direction, float maxDistance, LayerMask layerMask, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
		{
			var pointOffset = rotation * Vector3.up * (height - radius * 2f).Max() * 0.5f;
			length = Physics.CapsuleCastNonAlloc(origin + pointOffset, origin - pointOffset, radius, direction, hits, maxDistance, layerMask, queryTriggerInteraction);
		}
		public void CapsuleCast(Vector3 origin, Quaternion rotation, float radius, float height, Vector3 target, LayerMask layerMask, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
		{
			var pointOffset = rotation * Vector3.up * (height - radius * 2f).Max() * 0.5f;
			var delta = target - origin;
			length = Physics.CapsuleCastNonAlloc(origin + pointOffset, origin - pointOffset, radius, delta.normalized, hits, delta.magnitude, layerMask, queryTriggerInteraction);
		}
		public void CapsuleCast(CapsuleCollider capsule, Vector3 direction, float maxDistance, LayerMask layerMask, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
		{
			length = Physics.CapsuleCastNonAlloc(capsule.GetHeadPositionUncapped(), capsule.GetTailPositionUncapped(), capsule.radius, direction, hits, maxDistance, layerMask, queryTriggerInteraction);
		}
		public void CapsuleCast(CapsuleCollider capsule, Vector3 target, LayerMask layerMask, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
		{
			var origin = capsule.transform.position + capsule.center;
			var delta = target - origin;
			length = Physics.CapsuleCastNonAlloc(capsule.GetHeadPositionUncapped(), capsule.GetTailPositionUncapped(), capsule.radius, delta.normalized, hits, delta.magnitude, layerMask, queryTriggerInteraction);
		}
		public void CapsuleCast(Vector3 origin, CapsuleCollider capsule, Vector3 direction, float maxDistance, LayerMask layerMask, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
		{
			length = Physics.CapsuleCastNonAlloc(origin + capsule.GetHeadPositionUncappedLocal(), origin + capsule.GetTailPositionUncappedLocal(), capsule.radius, direction, hits, maxDistance, layerMask, queryTriggerInteraction);
		}
		public void CapsuleCast(Vector3 origin, CapsuleCollider capsule, Vector3 target, LayerMask layerMask, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
		{
			var delta = target - origin;
			length = Physics.CapsuleCastNonAlloc(origin + capsule.GetHeadPositionUncappedLocal(), origin + capsule.GetTailPositionUncappedLocal(), capsule.radius, delta.normalized, hits, delta.magnitude, layerMask, queryTriggerInteraction);
		}

		#endregion
	}

	#endregion
	#region RaycastUtils

	public static class RaycastUtils
	{
		public static Surface GetSurface(this RaycastHit hit) => hit.collider.GetSurface();
		public static Surface GetSurface(this RaycastHit2D hit) => hit.collider.GetSurface();
	}

	#endregion
}
