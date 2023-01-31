using JetBrains.Annotations;
using MainGame.UnityView.UI.Builder.BluePrint;

namespace MainGame.UnityView.UI.Builder.Unity
{
    public interface IUIBuilderObject
    {
        /// <summary>
        /// ブループリントから作られ場合、元のブループリントを保持する
        /// <see cref="UIBluePrintType.ArraySlot"/>から作られた一つのスロットなどはブループリントから作られてないので、nullになる
        /// TODO　これなんか良く無い感じがするので修正したい
        /// </summary>
        [CanBeNull] public IUIBluePrintElement BluePrintElement { get; }
        public void Initialize(IUIBluePrintElement bluePrintElement);
    }
}