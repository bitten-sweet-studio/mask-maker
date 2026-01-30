using System.Threading;
using TMPro;
using UnityEngine;
using Yarn.Markup;
using Yarn.Unity;

namespace MaskMaker.Scripts.YarnComponents
{
    public class LinePresenterTypewriterAudio : ActionMarkupHandler
    {
        [SerializeField] AudioSource typeWritring;
        [SerializeField] AudioClip typeFX;
        
        public override void OnPrepareForLine(MarkupParseResult line, TMP_Text text)
        {
            return;
        }

        public override void OnLineDisplayBegin(MarkupParseResult line, TMP_Text text)
        {
            return;
        }

        public override YarnTask OnCharacterWillAppear(int currentCharacterIndex, MarkupParseResult line, CancellationToken cancellationToken)
        {
            typeWritring.PlayOneShot(typeFX);
            return YarnTask.CompletedTask;
        }

        public override void OnLineDisplayComplete()
        {
            typeWritring.Stop();
        }

        public override void OnLineWillDismiss()
        { }
    }
}