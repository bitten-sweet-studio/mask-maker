using System;
using UnityEngine;
using Yarn.Unity;

public class ShopManager : MonoBehaviour
{
    [SerializeField] MaskShopCustomer customerObject;
    [SerializeField] DialogueRunner dialogRunner;
    [SerializeField] DialogJournalPresenter dialogJournal;

    void Awake()
    {
        customerObject.OnCustomerArive += HandleNewCustomerArived;
    }

    void HandleNewCustomerArived()
    {
        dialogJournal.ResetDialogHistory();
        dialogRunner.StartDialogue(customerObject.GetCustomerYarnNode());
    }

    public void CustomerLoop()
    {
        //wip area
        CustomerData dat = new CustomerData();
        dat.yarnNodeName = "Start";
        //end wip area
        customerObject.SetActiveCustomer(dat);
        //play anim?
        //await for events?
    }
}