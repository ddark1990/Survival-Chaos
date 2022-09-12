using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SurvivalChaos
{
    //debug helper script server auth move command
    public class UnitCommandGiver : MonoBehaviour
    {
        [SerializeField] private LayerMask layerMask;


        private void Update()
        {
            if (!Input.GetKeyDown(KeyCode.Mouse1)) { return; }

            Ray ray = SelectionManager.Instance.cam.ScreenPointToRay(Input.mousePosition);

            if (!Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, layerMask)) { return; }

            if (hit.collider.TryGetComponent<Selectable>(out Selectable selectable))
            {
                if (selectable.hasAuthority)
                {
                    //TryMove(hit.point);
                    TryDamage(selectable);
                    return;
                }

                //TryTarget(selectable);
                return;
            }

            //TryMove(hit.point);
        }

        private void TryDamage(Selectable selectable)
        {
            selectable.CmdApplyDamage(100);
        }

        private void TryMove(Vector3 point)
        {
            var unit = SelectionManager.Instance.CurrentlySelectedObjects[0].GetComponent<Unit>();

            //unit.GetUnitMovement().CmdMove(point);
            //print($"{unit}");

        }

        private void TryTarget(Selectable target)
        {
            var unit = SelectionManager.Instance.CurrentlySelectedObjects[0].GetComponent<Unit>();

            unit.CmdSetTarget(target.gameObject);
            print($"{unit}");

        }
    }

}
