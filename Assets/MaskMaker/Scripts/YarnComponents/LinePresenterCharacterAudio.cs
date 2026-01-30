using System.Threading;
using TMPro;
using UnityEngine;
using Yarn.Markup;
using Yarn.Unity;

namespace MaskMaker.Scripts.YarnComponents
{
    public class LinePresenterCharacterAudio : ActionMarkupHandler
    {
        [SerializeField] AudioSource loopAudioSource;
        AudioClip voiceClip;
        
        public override void OnPrepareForLine(MarkupParseResult line, TMP_Text text)
        {
            loopAudioSource.Play();
        }

        public override void OnLineDisplayBegin(MarkupParseResult line, TMP_Text text)
        { }

        public override YarnTask OnCharacterWillAppear(int currentCharacterIndex, MarkupParseResult line, CancellationToken cancellationToken)
        {
            loopAudioSource.Stop();
            return YarnTask.CompletedTask;
        }

        public override void OnLineDisplayComplete()
        { }

        public override void OnLineWillDismiss()
        { }
    }
}