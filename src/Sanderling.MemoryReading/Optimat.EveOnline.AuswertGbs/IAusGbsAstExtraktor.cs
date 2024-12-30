using Sanderling.Interface.MemoryStruct;

namespace Optimat.EveOnline.AuswertGbs;

public interface IAusGbsAstExtraktor
{
	IUIElement Extrakt(UINodeInfoInTree gbsAst);
}
