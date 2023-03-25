
/** CustomNodeGraphView.cs
*
*	Created by LIAM WOFFORD of CUBEROOT SOFTWARE, LLC.
*
*	Free to use or modify, with or without creditation,
*	under the Creative Commons 0 License.
*/

#region Includes

using System;
using System.Linq;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

using UnityEditor;
using UnityEditor.Experimental.GraphView;

using NodeData = Cuberoot.NodeGraphEditableObject.NodeData;
using EdgeData = Cuberoot.NodeGraphEditableObject.EdgeData;

#endregion

namespace Cuberoot.Editor
{
	/// <summary>
	/// __TODO_ANNOTATE__
	///</summary>

	public class CustomNodeGraphView : GraphView
	{
		#region Inner Classes

		[Serializable]

		private struct Clipboard : ISerializable
		{
			[SerializeField]
			public NodeData[] nodeData;

			[SerializeField]
			public EdgeData[] edgeData;

			[SerializeField]
			public Vector2 averagePosition;

			public Clipboard(IEnumerable<GraphElement> elements)
			{
				var __nodes = new List<NodeData>();
				var __edges = new List<EdgeData>();

				foreach (var iElement in elements)
				{
					if (iElement is CustomNode iNode)
						__nodes.Add(iNode);
					else if (iElement is Edge iEdge)
						__edges.Add(iEdge);
				}

				nodeData = __nodes.ToArray();
				edgeData = __edges.ToArray();

				averagePosition = __nodes.Select(i => i.Rect.position).Average();
			}
		}

		#endregion
		#region Data

		#region

		public UnityEvent OnModified;

		private NodeSearchWindow _searchWindow;

		private Vector2 _mousePosition;
		public Vector2 mousePosition => _mousePosition;

		#endregion

		#endregion
		#region Properties

		public virtual List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
		{
			var __result = new List<SearchTreeEntry>
			{
				new SearchTreeGroupEntry(new GUIContent("Available Nodes"), 0),
					new SearchTreeEntry(new GUIContent("Custom Node")) { level = 1, userData = typeof(CustomNode) },

			};

			return __result;
		}

		#endregion
		#region Methods

		#region Construction

		public CustomNodeGraphView()
		{
			OnModified = new UnityEvent();

			styleSheets.Add(Resources.Load<StyleSheet>("Stylesheets/GraphBackgroundDefault"));

			SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);

			this.AddManipulator(new ContentDragger());
			this.AddManipulator(new SelectionDragger());
			this.AddManipulator(new RectangleSelector());
			// this.AddManipulator(new ContextualMenuManipulator(_CreateContextMenu));

			deleteSelection += OnDelete;
			unserializeAndPaste += OnPaste;
			serializeGraphElements += OnCopy;

			RegisterCallback<MouseMoveEvent>(OnMouseMove);

			var __background = new GridBackground();
			Insert(0, __background);
			__background.StretchToParentSize();

			AddSearchWindow();

			this.StretchToParentSize();

			/**	Didn't work when I tried it
			*/
			// FrameAll();
		}

		#endregion

		#region Overrides

		// protected override bool canCutSelection => base.canCutSelection;

		protected override bool canPaste => true;

		// protected override bool canDuplicateSelection
		// {
		// 	get
		// 	{
		// 		foreach (var i in selection)
		// 			if (i is CustomNode iNode && iNode.IsPredefined)
		// 				return false;

		// 		return base.canDuplicateSelection;
		// 	}
		// }

		// protected override bool canDeleteSelection
		// {
		// 	get
		// 	{
		// 		foreach (var i in selection)
		// 		{
		// 			if (i is CustomNode iNode && iNode.IsPredefined)
		// 				return false;
		// 		}

		// 		return base.canDeleteSelection;
		// 	}
		// }

		// protected override bool canCopySelection => base.canCopySelection;



		public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
		{
			var __result = new List<Port>();

			ports.ForEach((port) =>
			{
				if (startPort != port && startPort.node != port.node)
					__result.Add(port);
			});

			return __result;
		}

		#endregion
		#region Context Menu

		private void _CreateContextMenu(ContextualMenuPopulateEvent context)
		{
			// CreateContextMenu(context);
			// context.menu.InsertSeparator("/", 1);
		}
		protected virtual void CreateContextMenu(ContextualMenuPopulateEvent context)
		{
			context.menu.InsertAction(0, "Create Node", (_) =>
			{
				// CreateNewNode<CustomNode>("New Node");
			});
		}

		private void AddSearchWindow()
		{
			_searchWindow = ScriptableObject.CreateInstance<NodeSearchWindow>();
			nodeCreationRequest = context => SearchWindow.Open(new SearchWindowContext(context.screenMousePosition), _searchWindow);
			_searchWindow.InitializeFor(this);
		}

		#endregion
		#region Node Handling

		#region Predefined Nodes

		public virtual void CreatePredefinedNodes() { }

		#endregion

		#region Creation

		public CustomNode CreateNewNode(Type type, Guid guid, Rect rect, bool invokeOnModified = true)
		{
			var __node = (CustomNode)Activator.CreateInstance(type);

			__node.Guid = guid;
			__node.title = __node.DefaultName;
			__node.SetPosition(rect);
			__node.InitializeFor(this);

			AddElement(__node);

			if (invokeOnModified)
				OnModified.Invoke();

			return __node;
		}
		public CustomNode CreateNewNode(Type type, Guid? guid = null, Rect? rect = null, string title = null, bool invokeOnModified = true)
		{
			var __node = (CustomNode)Activator.CreateInstance(type);

			if (guid != null)
				__node.Guid = guid.Value;
			if (rect != null)
				__node.SetPosition(rect.Value);
			if (title != null)
				__node.title = title;

			__node.InitializeFor(this);

			AddElement(__node);

			if (invokeOnModified)
				OnModified.Invoke();

			return __node;
		}
		public CustomNode CreateNewNodeAtCursor(Type type, Guid? guid = null, Vector2? size = null, string title = null, bool invokeOnModified = true)
		{
			var __node = CreateNewNode(type, guid, null, title, invokeOnModified);

			__node.SetPosition(new Rect(
				mousePosition,
				(size != null) ?
					size.Value :
					__node.GetPosition().size
			));

			return __node;
		}

		public CustomNode CreateNewNode(NodeGraphEditableObject.NodeData data, bool createNewGuid = false) =>
			CreateNewNode(
				Type.GetType(data.SubtypeName),
				createNewGuid ? Guid.Generate() : data.Guid,
				data.Rect, data.Title, false
			)
		;

		public T CreateNewNode<T>(Guid? guid = null, Rect? rect = null, string title = null, bool invokeOnModified = true)
		where T : CustomNode, new() =>
			(T)CreateNewNode(typeof(T), guid, rect, title, invokeOnModified);


		#endregion

		#region Dupe / Cut / Copy / Paste

		private string OnCopy(IEnumerable<GraphElement> elements)
		{
			ISerializable __clipboard = new Clipboard(elements);

			Editor.Clipboard.Copy(__clipboard);

			return GUIUtility.systemCopyBuffer;
		}

		private void OnPaste(string operationName, string data)
		{
			var __clipboard = Editor.Clipboard.PasteText<Clipboard>(data);

			OnPaste(__clipboard);
		}

		private void OnPaste(Clipboard clipboard)
		{
			/** <<============================================================>> **/

			var __guidLinks = new MapField<Guid, Guid>();

			/** <<============================================================>> **/

			var __nodes = new List<CustomNode>();

			var __deltaPosition = mousePosition - clipboard.averagePosition;

			foreach (var iNodeData in clipboard.nodeData)
			{
				var iNode = CreateNewNode(iNodeData, true);
				iNode.SetPositionOnly(iNodeData.Rect.position + __deltaPosition);

				__nodes.Add(iNode);
				__guidLinks.Add((iNodeData.Guid, iNode.Guid));
			}

			/** <<============================================================>> **/

			var __edges = new List<Edge>();

			foreach (var iEdgeData in clipboard.edgeData)
			{
				var __linkData = iEdgeData;

				__linkData.nPort.NodeGuid = __guidLinks.TryGetValue(iEdgeData.nPort.NodeGuid);
				__linkData.oPort.NodeGuid = __guidLinks.TryGetValue(iEdgeData.oPort.NodeGuid);

				try
				{ __edges.Add(CreateEdge(__linkData)); }
				catch
				{ continue; }
			}

			/** <<============================================================>> **/

			var __newSelection = new List<ISelectable>();

			__newSelection.AddRange(__nodes);
			__newSelection.AddRange(__edges);

			this.SetSelection(__newSelection);
		}

		#endregion

		#region Modification

		public Edge CreateEdge(EdgeData edge)
		{
			var nPort = FindNode(edge.nPort.NodeGuid).FindPort(edge.nPort.PortName);
			var oPort = FindNode(edge.oPort.NodeGuid).FindPort(edge.oPort.PortName);

			return ConnectPorts(nPort, oPort);
		}

		public Edge ConnectPorts(Port input, Port output)
		{
			var __edge = new Edge
			{
				input = input,
				output = output
			};

			__edge?.input.Connect(__edge);
			__edge?.output.Connect(__edge);

			Add(__edge);

			return __edge;
		}

		#endregion

		#region Deletion

		private void OnDelete(string operationName, AskUser askUser)
		{
			var __toRemove = new List<GraphElement>();
			foreach (GraphElement i in selection)
			{
				if (i is CustomNode iNode)
				{
					if (iNode.IsPredefined)
						continue;

					var __connectedEdges = iNode.GetAllConnectedEdges();

					__toRemove.AddRange(__connectedEdges);
				}

				__toRemove.Add(i);
			}

			DeleteElements(__toRemove);
		}

		public void ClearAllNodes()
		{
			var __nodes = nodes.Cast<CustomNode>();
			foreach (var iNode in __nodes)
			{
				if (iNode != null)
				{
					if (iNode.IsPredefined)
						continue;
				}

				edges.Where(i => i.input.node == iNode).ToList()
					.ForEach(iEdge => { RemoveElement(iEdge); });

				RemoveElement(iNode);
			}
		}

		public void ClearAllNodes_WithPrompt()
		{
			if (nodes.Any())
				try { Utils.PromptConfirmation("Are you sure you want to clear ALL nodes on this graph?"); }
				catch { return; }

			ClearAllNodes();
		}




		#endregion

		#region Query

		public CustomNode FindNode(Guid guid)
		{
			foreach (var i in nodes)
				if (i is CustomNode iNode && iNode.Guid == guid)
					return iNode;

			throw new System.Exception($"Node with GUID {guid} was not found in {this.name}.");
		}

		#endregion

		#endregion
		#region Utils

		public void SetViewPosition(Vector2 position) =>
			UpdateViewTransform(position, this.viewTransform.scale);

		private void OnMouseMove(MouseMoveEvent context)
		{
			_mousePosition = viewTransform.matrix.inverse.MultiplyPoint(context.localMousePosition);
		}

		#endregion

		#endregion
	}
}
