
/** Surface.cs
*
*	Created by LIAM WOFFORD.
*
*	Free to use or modify, with or without creditation,
*	under the Creative Commons 0 License.
*/

#region Includes

using System.Collections;
using System.Collections.Generic;

using UnityEngine;

#endregion

namespace Mithril
{
	/// <summary>
	/// A Surface is Mithril's custom Physical Material class. It contains simple physics information for pawns to use when moving and allows for other things such as colliders with associated sound effects.
	///</summary>

	[CreateAssetMenu(fileName = "New Surface", menuName = "Mithril/Physics/Surface", order = 50)]

	public class Surface : ScriptableObject
	{
		#region Fields

		/// <summary>
		/// Pawns with a FrictionMovement component will have their acceleration and deceleration multiplied by this amount.
		///</summary>

		[Tooltip("Pawns with a FrictionMovement component will have their acceleration and deceleration multiplied by this amount.")]
		[SerializeField]

		public float frictionScale = 1f;

		/// <summary>
		/// The "viscosity" of this surface, pawns with a <see cref="WalkMovement"/> component will have their <see cref="WalkMovement.maxSpeed"/> multiplied by this amount.
		///</summary>

		[Tooltip("The \"viscosity\" of this surface, pawns with a WalkMovement component will have their Max Walk Speed multiplied by this amount.")]
		[SerializeField]

		public float walkSpeedScale = 1f;

		#endregion
	}

	public static class SurfaceExtensions
	{
		#region GetSurface

		/// <returns>
		/// The Surface object (if one exists) associated with the given <paramref name="gameObject"/>.
		///</returns>

		public static Surface GetSurface(this GameObject gameObject)
		{
			SurfaceFilter filter = gameObject.GetComponent<SurfaceFilter>();

			if (filter != null)
				return filter.Surface;
			return default;
		}

		/// <returns>
		/// The Surface object (if one exists) associated with the given <paramref name="component"/>.
		///</returns>

		public static Surface GetSurface(this Component component) => GetSurface(component.gameObject);

		#endregion
	}
}
