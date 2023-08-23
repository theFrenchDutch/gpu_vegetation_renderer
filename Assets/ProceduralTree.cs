using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

[ExecuteInEditMode]
public class ProceduralTree : MonoBehaviour
{
	// Variables
	public static ProceduralTree Instance;
	public bool resetButton = false;
	public int treeDepth = 4;
	public float rootLength = 10.0f;
	public GameObject treeNodePrefab;
	public Mesh branchMesh;
	public Mesh leafMesh;
	public Material branchMaterial;
	public Material leafMaterial;
	private TreeNode root;



	void Awake()
	{
		Instance = this;
	}

	void OnEnable()
	{
		InitTree();
	}

	void OnDisable()
	{
		if (root != null && root.nodeObject != null)
			DestroyImmediate(root.nodeObject);
	}

	void Update()
	{
		if (resetButton == true)
		{
			resetButton = false;
			if (root != null && root.nodeObject != null)
				DestroyImmediate(root.nodeObject);
			InitTree();
		}
	} 



	public void InitTree()
	{
		if (Instance == null)
			Instance = this;

		root = new TreeNode(0, null, Vector3.up, rootLength);
		root.UpdateNodeObject();
		for (int i = 0; i < treeDepth; i++)
			SubdivideWholeTreeOnce();
	}

	public List<TreeNode> FindLeafNodes()
	{
		List<TreeNode> leafNodes = new List<TreeNode>();
		if (root == null)
			return leafNodes;

		FindLeafNodesRecursive(ref leafNodes, root);
		return leafNodes;
	}

	public void FindLeafNodesRecursive(ref List<TreeNode> leaves, TreeNode current)
	{
		if (current.leftChild == null && current.rightChild == null)
			leaves.Add(current);
		else
		{
			FindLeafNodesRecursive(ref leaves, current.leftChild);
			FindLeafNodesRecursive(ref leaves, current.rightChild);
		}
	}

	public void SubdivideWholeTreeOnce()
	{
		List<TreeNode> leafNodes = FindLeafNodes();
		for (int i = 0; i < leafNodes.Count; i++)
			SubdivideNode(leafNodes[i]);
	}

	public void MergeWholeTreeOnce()
	{
		List<TreeNode> leafNodes = FindLeafNodes();
		for (int i = 0; i < leafNodes.Count; i++)
		{
			if (leafNodes[i] != null)
				MergeNode(leafNodes[i].parent);
		}
	}

	public void SubdivideNode(TreeNode node)
	{
		TreeNode left = new TreeNode(node.depth + 1, node, Vector3.Normalize(node.direction * 1.0f + UnityEngine.Random.onUnitSphere), node.length / 2.0f);
		node.leftChild = left;
		left.UpdateNodeObject();
		TreeNode right = new TreeNode(node.depth + 1, node, Vector3.Normalize(node.direction * 1.0f + UnityEngine.Random.onUnitSphere), node.length / 2.0f);
		node.rightChild = right;
		right.UpdateNodeObject();
		node.UpdateNodeObject();
	}

	public void MergeNode(TreeNode node)
	{
		DestroyImmediate(node.leftChild.nodeObject);
		node.leftChild = null;
		DestroyImmediate(node.rightChild.nodeObject);
		node.rightChild = null;
		node.UpdateNodeObject();
	}




	// Structures
	public class TreeNode
	{
		public int depth;
		public TreeNode parent;
		public TreeNode leftChild;
		public TreeNode rightChild;

		public float length;
		public Vector3 direction;
		public GameObject nodeObject;
		public GameObject nodeRenderer;

		public TreeNode(int d, TreeNode p, Vector3 dir, float l = 0.0f, TreeNode lC = null, TreeNode rC = null)
		{
			depth = d;
			parent = p;
			direction = dir;
			leftChild = lC;
			rightChild = rC;
			length = l;
		}

		public void UpdateNodeObject()
		{
			if (nodeObject == null)
				nodeObject = new GameObject("TreeNode");
			if (parent != null)
				nodeObject.transform.parent = parent.nodeObject.transform;
			if (nodeRenderer == null)
			{
				nodeRenderer = Instantiate(ProceduralTree.Instance.treeNodePrefab);
				nodeRenderer.transform.parent = nodeObject.transform;
			}

			// Is Leaf
			if (leftChild == null && rightChild == null)
			{
				nodeRenderer.GetComponent<MeshFilter>().sharedMesh = ProceduralTree.Instance.leafMesh;
				nodeRenderer.GetComponent<MeshRenderer>().sharedMaterial = ProceduralTree.Instance.leafMaterial;
			}
			else // Is Branch
			{
				nodeRenderer.GetComponent<MeshFilter>().sharedMesh = ProceduralTree.Instance.branchMesh;
				nodeRenderer.GetComponent<MeshRenderer>().sharedMaterial = ProceduralTree.Instance.branchMaterial;
			}

			UpdateNodeObjectTransform();
		}

		public void UpdateNodeObjectTransform()
		{
			if (parent != null)
				nodeObject.transform.position = parent.nodeObject.transform.position + parent.length * parent.direction;
			else
				nodeObject.transform.position = ProceduralTree.Instance.transform.position;
			nodeObject.transform.LookAt(nodeObject.transform.position + direction);
			
			// Is Leaf
			if (leftChild == null && rightChild == null)
			{
				nodeRenderer.transform.localScale = Vector3.one * 100.0f / math.pow(2.0f, depth);
			}
			else // Is Branch
			{
				nodeRenderer.transform.localRotation = Quaternion.Euler(90, 0, 0);
				nodeRenderer.transform.localScale = new Vector3(1.0f / math.pow(2.0f, depth), length / 2.0f, 1.0f / math.pow(2.0f, depth));
				nodeRenderer.transform.position += direction * length / 2.0f;
			}
		}
	}
}