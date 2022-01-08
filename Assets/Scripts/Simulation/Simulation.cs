using System.Collections.Generic;
using UnityEngine;

public class Simulation : MonoBehaviour
{
	// Declare variables
	public float pointRadius;
	public float stickThickness;
	bool drawingStick;
	int stickStartIndex;

	public Color pointCol;
	public Color fixedPointCol;
	public Color lineCol;
	public Color lineDrawCol;
	public bool autoStickMode;
	public bool constrainStickMinLength = true;
	public bool simulating;
	Vector2 cutPosOld;

	public float gravity = 10;
	public int solveIterations = 5;
	protected List<Point> points;
	protected List<Stick> sticks;
	int[] order;

	// Initiate lists for points and sticks
	protected virtual void Start()
	{

		if (points == null)
		{
			points = new List<Point>();
		}
		if (sticks == null)
		{
			sticks = new List<Stick>();
		}

		CreateOrderArray();
	}

	// Method for simulation
	void Simulate()
	{
        // Loops through points and checks if they should be moved
        foreach (Point p in points)
		{
			if (!p.locked)
			{
				// Using what looks like a combination of verlet integration and gravitional acceleration

				Vector2 positionBeforeUpdate = p.position;
				p.position += p.position - p.prevPosition; // <-- verlet integration?
				p.position += Vector2.down * gravity * Time.deltaTime * Time.deltaTime; // <-- gravitational acceleration (units/second squared)
				p.prevPosition = positionBeforeUpdate;
			}
		}

		// Loops through sticks and checks if they should be moved
		for (int i = 0; i < solveIterations; i++)
		{
			for (int s = 0; s < sticks.Count; s++)
			{
				Stick stick = sticks[order[s]];

				// skips if stick is "cut"
				if (stick.dead)
				{
					continue;
				}

				Vector2 stickCentre = (stick.pointA.position + stick.pointB.position) / 2;
				Vector2 stickDir = (stick.pointA.position - stick.pointB.position).normalized;
				float length = (stick.pointA.position - stick.pointB.position).magnitude;

				// constrains stick length by setting point position via the stick's center, direction and length
				if (length > stick.length || constrainStickMinLength)
				{
					if (!stick.pointA.locked)
					{
						stick.pointA.position = stickCentre + stickDir * stick.length / 2;
					}
					if (!stick.pointB.locked)
					{
						stick.pointB.position = stickCentre - stickDir * stick.length / 2;
					}
				}

			}
		}
	}

	// create point object
	[System.Serializable]
	public class Point
	{
		public Vector2 position, prevPosition;
		public bool locked;
	}

	// create stick object
	[System.Serializable]
	public class Stick
	{
		public Point pointA, pointB;
		public float length;
		public bool dead;

		public Stick(Point pointA, Point pointB)
		{
			this.pointA = pointA;
			this.pointB = pointB;
			length = Vector2.Distance(pointA.position, pointB.position);
		}
	}

	// Input handling method
	protected virtual void HandleInput(Vector2 mousePos)
	{
		// Check if space is inputted
		if (Input.GetKeyDown(KeyCode.Space))
		{
			simulating = !simulating;
		}
		if (simulating)
		{
			if (Input.GetKeyDown(KeyCode.E))
			{
				cutPosOld = mousePos;
			}
			if (Input.GetKey(KeyCode.E))
			{
				Cut(cutPosOld, mousePos);
				cutPosOld = mousePos;
			}

			for (int k = 0; k < points.Count; k++)
			{
				if (points[k].position.y < -20)
                {
					points.Remove(points[k]);
					CreateOrderArray();
				}
			}
		}
		else
		{

			int i = MouseOverPointIndex(mousePos);
			bool mouseOverPoint = i != -1;

			if (Input.GetMouseButtonDown(1) && mouseOverPoint)
			{
				points[i].locked = !points[i].locked;
			}

			// Added feature to remove points and sticks by holding "E"
			if (Input.GetKeyDown(KeyCode.E)) //&& mouseOverPoint)
            {
				cutPosOld = mousePos;
			}
			if (Input.GetKey(KeyCode.E))
			{
				Cut(cutPosOld, mousePos);
				cutPosOld = mousePos;

				if (i != -1)
                {
					points.RemoveAt(i);
				}
			}

			// Clear all points/sticks by pressing "C"
			if (Input.GetKey(KeyCode.C))
            {
				points = new List<Point>();

				sticks = new List<Stick>();

				CreateOrderArray();
			}

			if (Input.GetMouseButtonDown(0))
			{
				if (mouseOverPoint)
				{
					drawingStick = true;
					stickStartIndex = i;
				}
				else if (Menu.mouseOverMenu == false)
				{
					points.Add(new Point() { position = mousePos, prevPosition = mousePos });
					Debug.Log(mousePos);
				}
			}

			if (Input.GetMouseButtonUp(0))
			{
				if (mouseOverPoint && drawingStick)
				{
					if (i != stickStartIndex)
					{
						sticks.Add(new Stick(points[stickStartIndex], points[i]));
						CreateOrderArray();
					}
				}
				drawingStick = false;
			}

			if (autoStickMode || Input.GetKey(KeyCode.LeftShift))
			{
				sticks.Clear();
				for (int k = 0; k < points.Count - 1; k++)
				{
					sticks.Add(new Stick(points[k], points[k + 1]));
					CreateOrderArray();
				}
				autoStickMode = false;
			}
		}
	}

	int MouseOverPointIndex(Vector2 mousePos)
	{
		for (int i = 0; i < points.Count; i++)
		{
			float dst = (points[i].position - mousePos).magnitude;

			if (dst < pointRadius)
			{
				return i;
			}
		}
		return -1;
	}

	void Draw()
	{

		foreach (Point point in points)
		{
			Visualizer.SetColour((point.locked) ? fixedPointCol : pointCol);
			Visualizer.DrawSphere(point.position, pointRadius);
		}

		Visualizer.SetColour(lineCol);
		foreach (Stick stick in sticks)
		{
			if (!stick.dead)
				Visualizer.DrawLine(stick.pointA.position, stick.pointB.position, stickThickness);
		}

		if (drawingStick)
		{
			Visualizer.SetColour(lineDrawCol);
			Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
			Visualizer.DrawLine(points[stickStartIndex].position, mousePos, stickThickness);
		}
	}


	void Update()
	{
		HandleInput(Camera.main.ScreenToWorldPoint(Input.mousePosition));
		if (simulating)
		{
			Simulate();
		}
	}

	void LateUpdate()
	{
		Draw();
	}

	public static T[] ShuffleArray<T>(T[] array, System.Random prng)
	{

		int elementsRemainingToShuffle = array.Length;
		int randomIndex = 0;

		while (elementsRemainingToShuffle > 1)
		{

			// Choose a random element from array
			randomIndex = prng.Next(0, elementsRemainingToShuffle);
			T chosenElement = array[randomIndex];

			// Swap the randomly chosen element with the last unshuffled element in the array
			elementsRemainingToShuffle--;
			array[randomIndex] = array[elementsRemainingToShuffle];
			array[elementsRemainingToShuffle] = chosenElement;
		}

		return array;
	}

	protected void CreateOrderArray()
	{
		order = new int[sticks.Count];
		for (int i = 0; i < order.Length; i++)
		{
			order[i] = i;
		}
		ShuffleArray(order, new System.Random());
	}

	void Cut(Vector2 start, Vector2 end)
	{
		for (int i = sticks.Count - 1; i >= 0; i--)
		{
			if (SegmentsIntersect(start, end, sticks[i].pointA.position, sticks[i].pointB.position))
			{
				sticks[i].dead = true;
			}
		}
	}

	public static bool SegmentsIntersect(Vector2 a1, Vector2 a2, Vector2 b1, Vector2 b2)
	{
		float d = (b2.x - b1.x) * (a1.y - a2.y) - (a1.x - a2.x) * (b2.y - b1.y);
		if (d == 0)
			return false;
		float t = ((b1.y - b2.y) * (a1.x - b1.x) + (b2.x - b1.x) * (a1.y - b1.y)) / d;
		float u = ((a1.y - a2.y) * (a1.x - b1.x) + (a2.x - a1.x) * (a1.y - b1.y)) / d;

		return t >= 0 && t <= 1 && u >= 0 && u <= 1;
	}

	void OnDrawGizmos()
	{
		//Gizmos.DrawWireCube(Vector3.zero, boundsSize);
	}

	public void Preset1()
    {
		points = new List<Point>();
		
		sticks = new List<Stick>();

		CreateOrderArray();

		Vector2 position = new Vector2(0f, 1f);
		points.Add(new Point() { position = position, prevPosition = position, locked = true });
		position = new Vector2(0.1f, 3f);
		points.Add(new Point() { position = position, prevPosition = position, locked = false });
		position = new Vector2(0.2f, 5f);
		points.Add(new Point() { position = position, prevPosition = position, locked = false });

		autoStickMode = true;

		Debug.Log("preset 1");
	}

	public void Preset2()
	{
		points = new List<Point>();

		sticks = new List<Stick>();

		CreateOrderArray();

		Vector2 position = new Vector2(0f, 5f);
		points.Add(new Point() { position = position, prevPosition = position, locked = true });
		position = new Vector2(-0.3f, 4f);
		points.Add(new Point() { position = position, prevPosition = position, locked = false });
		position = new Vector2(-0.8f, 3.2f);
		points.Add(new Point() { position = position, prevPosition = position, locked = false });
		position = new Vector2(-1.5f, 2.5f);
		points.Add(new Point() { position = position, prevPosition = position, locked = false });
		position = new Vector2(-2.5f, 2f);
		points.Add(new Point() { position = position, prevPosition = position, locked = false });
		position = new Vector2(-3.7f, 1.7f);
		points.Add(new Point() { position = position, prevPosition = position, locked = false });

		autoStickMode = true;

		Debug.Log("preset 2");
	}

	public void Preset3()
	{
		points = new List<Point>();

		sticks = new List<Stick>();

		CreateOrderArray();

		Vector2 position = new Vector2(-4f, 5f);
		points.Add(new Point() { position = position, prevPosition = position, locked = true });
		position = new Vector2(-2.7f, 4f);
		points.Add(new Point() { position = position, prevPosition = position, locked = false });
		position = new Vector2(-1.3f, 3.5f);
		points.Add(new Point() { position = position, prevPosition = position, locked = false });
		position = new Vector2(0f, 3.2f);
		points.Add(new Point() { position = position, prevPosition = position, locked = false });
		position = new Vector2(1.3f, 3.5f);
		points.Add(new Point() { position = position, prevPosition = position, locked = false });
		position = new Vector2(2.7f, 4f);
		points.Add(new Point() { position = position, prevPosition = position, locked = false });
		position = new Vector2(4f, 5f);
		points.Add(new Point() { position = position, prevPosition = position, locked = true });
		position = new Vector2(-1f, 1f);
		points.Add(new Point() { position = position, prevPosition = position, locked = false });

		sticks.Add(new Stick(points[0], points[1]));
		sticks.Add(new Stick(points[1], points[2]));
		sticks.Add(new Stick(points[2], points[3]));
		sticks.Add(new Stick(points[3], points[4]));
		sticks.Add(new Stick(points[4], points[5]));
		sticks.Add(new Stick(points[5], points[6]));
		sticks.Add(new Stick(points[3], points[7]));
		CreateOrderArray();

		Debug.Log("preset 3");
	}

	public void Preset4()
	{
		points = new List<Point>();

		sticks = new List<Stick>();

		CreateOrderArray();

		Vector2 position = new Vector2(0f, 5f);
		points.Add(new Point() { position = position, prevPosition = position, locked = true });
		position = new Vector2(-1f, 5f);
		points.Add(new Point() { position = position, prevPosition = position, locked = false });
		position = new Vector2(-2f, 5f);
		points.Add(new Point() { position = position, prevPosition = position, locked = false });
		position = new Vector2(-2.5f, 5.5f);
		points.Add(new Point() { position = position, prevPosition = position, locked = false });
		position = new Vector2(-3f, 5f);
		points.Add(new Point() { position = position, prevPosition = position, locked = false });
		position = new Vector2(-2.5f, 4.5f);
		points.Add(new Point() { position = position, prevPosition = position, locked = false });

		sticks.Add(new Stick(points[0], points[1]));
		sticks.Add(new Stick(points[1], points[2]));
		sticks.Add(new Stick(points[2], points[3]));
		sticks.Add(new Stick(points[3], points[4]));
		sticks.Add(new Stick(points[4], points[5]));
		sticks.Add(new Stick(points[5], points[2]));
		sticks.Add(new Stick(points[5], points[3]));
		CreateOrderArray();

		Debug.Log("preset 4");
	}

	public void Preset5()
	{
		points = new List<Point>();

		sticks = new List<Stick>();

		CreateOrderArray();

		Vector2 position = new Vector2(4.5f, 5f);
		points.Add(new Point() { position = position, prevPosition = position, locked = true });
		position = new Vector2(3f, 3.5f);
		points.Add(new Point() { position = position, prevPosition = position, locked = false });
		position = new Vector2(0f, 5f);
		points.Add(new Point() { position = position, prevPosition = position, locked = true });
		position = new Vector2(-1.5f, 3.5f);
		points.Add(new Point() { position = position, prevPosition = position, locked = false });
		position = new Vector2(-3f, 2f);
		points.Add(new Point() { position = position, prevPosition = position, locked = false });
		position = new Vector2(-4.5f, 5f);
		points.Add(new Point() { position = position, prevPosition = position, locked = true });
		position = new Vector2(-6f, 3.5f);
		points.Add(new Point() { position = position, prevPosition = position, locked = false });
		position = new Vector2(-7.5f, 2f);
		points.Add(new Point() { position = position, prevPosition = position, locked = false });
		position = new Vector2(-9f, 0.5f);
		points.Add(new Point() { position = position, prevPosition = position, locked = false });

		sticks.Add(new Stick(points[0], points[1]));
		sticks.Add(new Stick(points[2], points[3]));
		sticks.Add(new Stick(points[3], points[4]));
		sticks.Add(new Stick(points[5], points[6]));
		sticks.Add(new Stick(points[6], points[7]));
		sticks.Add(new Stick(points[7], points[8]));
		CreateOrderArray();

		Debug.Log("preset 5");
	}

	public void Preset6()
	{
		points = new List<Point>();

		sticks = new List<Stick>();

		CreateOrderArray();

		Vector2 position = new Vector2(-4f, 2f);
		points.Add(new Point() { position = position, prevPosition = position, locked = true });
		position = new Vector2(-2.7f, 1f);
		points.Add(new Point() { position = position, prevPosition = position, locked = false });
		position = new Vector2(-1.3f, 0.5f);
		points.Add(new Point() { position = position, prevPosition = position, locked = false });
		position = new Vector2(0f, 0.2f);
		points.Add(new Point() { position = position, prevPosition = position, locked = false });
		position = new Vector2(1.3f, 0.5f);
		points.Add(new Point() { position = position, prevPosition = position, locked = false });
		position = new Vector2(2.7f, 1f);
		points.Add(new Point() { position = position, prevPosition = position, locked = false });
		position = new Vector2(4f, 2f);
		points.Add(new Point() { position = position, prevPosition = position, locked = true });
		position = new Vector2(0f, 2.2f);
		points.Add(new Point() { position = position, prevPosition = position, locked = true });
		position = new Vector2(-2f, 3.5f);
		points.Add(new Point() { position = position, prevPosition = position, locked = true });
		position = new Vector2(2f, 3.5f);
		points.Add(new Point() { position = position, prevPosition = position, locked = true });

		sticks.Add(new Stick(points[0], points[1]));
		sticks.Add(new Stick(points[1], points[2]));
		sticks.Add(new Stick(points[2], points[3]));
		sticks.Add(new Stick(points[3], points[4]));
		sticks.Add(new Stick(points[4], points[5]));
		sticks.Add(new Stick(points[5], points[6]));
		CreateOrderArray();

		Debug.Log("preset 6");
	}
}
