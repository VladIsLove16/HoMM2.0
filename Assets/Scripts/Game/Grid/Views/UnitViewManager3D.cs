using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;

public class UnitViewManager3D : MonoBehaviour
{
	[Inject] private UnitViewFactory _factory;
	[Inject] private IGridCellRenderer _renderer;

	private Dictionary<UnitModel, UnitView3D> _views = new();
	private const float MoveTime = 1.2f;

	public void Add(UnitModel model)
	{
		var view = _factory.Create(model);
		if (view != null)
			_views[model] = view;
	}

	public void Remove(UnitModel model)
	{
		if (_views.TryGetValue(model, out var view))
		{
			Destroy(view.gameObject, 5f);
			_views.Remove(model);
		}
	}

	public void MoveAlongRoute(UnitModel model, List<Vector2Int> path)
	{
		if (_views.TryGetValue(model, out var view))
		{
			List<Vector3> points = path.Select(p => _renderer.ToWorld(p.x, p.y)).ToList();
			StartCoroutine(AnimatePath(view.transform, points));
		}
	}

	public void Swap(UnitModel a, UnitModel b)
	{
		if (_views.TryGetValue(a, out var viewA) && _views.TryGetValue(b, out var viewB))
		{
			StartCoroutine(AnimateSwap(viewA.transform, viewB.transform));
		}
	}

	private IEnumerator AnimatePath(Transform unit, List<Vector3> path)
	{
		foreach (var target in path)
		{
			Vector3 start = unit.position;
			float t = 0f;
			while (t < MoveTime)
			{
				unit.position = Vector3.Lerp(start, target, t / MoveTime);
				t += Time.deltaTime;
				yield return null;
			}
			unit.position = target;
		}
	}

	private IEnumerator AnimateSwap(Transform a, Transform b)
	{
		Vector3 posA = a.position;
		Vector3 posB = b.position;
		float t = 0f;
		while (t < MoveTime)
		{
			a.position = Vector3.Lerp(posA, posB, t / MoveTime);
			b.position = Vector3.Lerp(posB, posA, t / MoveTime);
			t += Time.deltaTime;
			yield return null;
		}
		a.position = posB;
		b.position = posA;
	}
}
using System.Collections.Generic;
using UnityEngine;
using Zenject;
