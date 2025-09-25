using Microsoft.AspNetCore.Mvc;

public class ViewDataTestController : Controller
{
    private readonly ScopedGuidService _scoped1;
    private readonly ScopedGuidService _scoped2;
    private readonly TransientGuidService _transient1;
    private readonly TransientGuidService _transient2;
    private readonly SingletonGuidService _singleton1;
    private readonly SingletonGuidService _singleton2;

    public ViewDataTestController(ScopedGuidService scoped1, ScopedGuidService scoped2, TransientGuidService transient1, TransientGuidService transient2, SingletonGuidService singleton1, SingletonGuidService singleton2)
    {
        _scoped1 = scoped1;
        _scoped2 = scoped2;
        _transient1 = transient1;
        _transient2 = transient2;
        _singleton1 = singleton1;
        _singleton2 = singleton2;
    }


    [HttpGet]
    public IActionResult GetDIID()
    {
        ViewBag.ScopedGuid_First = _scoped1.GetGuid();
        ViewBag.ScopedGuid_Second = _scoped2.GetGuid();
        ViewBag.TransientGuid_First = _transient1.GetGuid();
        ViewBag.TransientGuid_Second = _transient2.GetGuid();
        ViewBag.SingletonGuid_First = _singleton1.GetGuid();
        ViewBag.SingletonGuid_Second = _singleton2.GetGuid();
        return View();
    }
    public IActionResult Index()
    {
        ViewData["vd1"] = "This is ViewData1 Message";
        ViewData["vd2"] = "This is ViewData2 Message";

        ViewBag.vb1 = "This is ViewBag1 Message";
        ViewBag.vb2 = "This is ViewBag2 Message";

        TempData["temp1"] = "This is TempData1 Message";
        TempData["temp2"] = "This is TempData2 Message";
        TempData["temp3"] = "This is TempData3 Message";

        return View();
    }

    public IActionResult OneRedirect()
    {
        return View();
    }

    public IActionResult TwoRedirect()
    {
        return View();
    }

    
}