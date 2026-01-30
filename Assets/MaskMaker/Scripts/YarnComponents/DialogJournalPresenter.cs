using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Yarn.Unity;

public class DialogJournalPresenter : DialoguePresenterBase
{
    struct JournalDialogCache
    {
        public string Character;
        public string Dialogue;
    }

    [SerializeField] TextMeshProUGUI journalText;
    
    List<JournalDialogCache> journalCache;
    
    //todo: techdebt eh legal termos o controle via `JournalCache` e so atualizar quando a UI for `Displayed`
    string hyperMonolithOfDialogues = String.Empty;

    void Awake()
    {
        journalCache = new();
    }

    public void ResetDialogHistory()
    {
        journalCache.Clear();
        hyperMonolithOfDialogues = "";
    }

    public override YarnTask RunLineAsync(LocalizedLine line, LineCancellationToken token)
    {
        journalCache.Add(new JournalDialogCache { 
            Character = line.CharacterName, Dialogue = line.TextWithoutCharacterName.Text
        });

        hyperMonolithOfDialogues += $"{line.CharacterName}: {line.TextWithoutCharacterName.Text} \n";
        journalText.text = hyperMonolithOfDialogues;
        return YarnTask.CompletedTask;
    }

    public override YarnTask OnDialogueStartedAsync()
    {
        return YarnTask.CompletedTask;
    }

    public override YarnTask OnDialogueCompleteAsync()
    {
        return YarnTask.CompletedTask;
    }
}