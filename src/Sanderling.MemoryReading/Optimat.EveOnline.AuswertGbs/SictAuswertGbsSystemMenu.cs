using System;
using BotEngine.Common;
using Sanderling.Interface.MemoryStruct;

namespace Optimat.EveOnline.AuswertGbs;

public class SictAuswertGbsSystemMenu
{
	public readonly UINodeInfoInTree SystemMenuAst;

	public UINodeInfoInTree AstMainContainer { get; private set; }

	public UINodeInfoInTree AstHeaderButtonClose { get; private set; }

	public IWindow Ergeebnis { get; private set; }

	public SictAuswertGbsSystemMenu(UINodeInfoInTree systemMenuAst)
	{
		SystemMenuAst = systemMenuAst;
	}

	public virtual void Berecne()
	{
		//IL_0129: Unknown result type (might be due to invalid IL or missing references)
		//IL_012e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0140: Expected O, but got Unknown
		UINodeInfoInTree systemMenuAst = SystemMenuAst;
		if (systemMenuAst == null || true != systemMenuAst.VisibleIncludingInheritance)
		{
			return;
		}
		AstMainContainer = systemMenuAst.FirstMatchingNodeFromSubtreeBreadthFirst((UINodeInfoInTree kandidaat) => kandidaat.PyObjTypNameIsContainer() && "sysmenu".EqualsIgnoreCase(kandidaat.Name), 2, 1);
		if (AstMainContainer?.VisibleIncludingInheritance ?? false)
		{
			AstHeaderButtonClose = AstMainContainer.FirstMatchingNodeFromSubtreeBreadthFirst((UINodeInfoInTree kandidaat) => string.Equals("Icon", kandidaat.PyObjTypName, StringComparison.InvariantCultureIgnoreCase) && true == kandidaat.VisibleIncludingInheritance, 2, 1);
			Sprite val = AstHeaderButtonClose?.AlsSprite();
			Window val2 = systemMenuAst.Window(true, null, null, (Sprite[])(object)new Sprite[1] { val });
			Ergeebnis = (IWindow)new Window((IUIElement)(object)val2)
			{
				isModal = true
			};
		}
	}
}
