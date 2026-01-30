using System;
using UnityEngine;

public class MaskShopCustomer : MonoBehaviour
{
    public event Action OnMaskDelivered;
    public event Action OnCustomerArive; //todo: limpar historico de dialogo com `DialogJournalPresenter.ResetDialog`

    [SerializeField, Range(0, 10)] float maskAffinity;
    CustomerData currentCustomer;

    public void SetActiveCustomer(CustomerData customer) 
    {
        currentCustomer = customer;
        OnCustomerArive?.Invoke();
    }

    public string GetCustomerYarnNode() => currentCustomer.yarnNodeName;
}