//using Android.Views;
//using Android.Widget;
//using Microsoft.Maui.Controls;
//using Microsoft.Maui.Controls.Platform;
//using TimeTracker.Mobile.Effects;

//[assembly: ResolutionGroupName("TimeTracker")]
//[assembly: ExportEffect(typeof(TimeTracker.Mobile.Platforms.Android.Effects.NoCopyPasteEffect), nameof(NoCopyPasteEffect))]
//namespace TimeTracker.Mobile.Platforms.Android.Effects;

//public class NoCopyPasteEffect : PlatformEffect
//{
//    protected override void OnAttached()
//    {
//        if (Control is EditText editText)
//        {
//            if (OperatingSystem.IsAndroidVersionAtLeast(23))
//            {
//                editText.CustomInsertionActionModeCallback = new NoPasteActionModeCallback();
//                editText.CustomSelectionActionModeCallback = new NoPasteActionModeCallback();
//            }
//            editText.LongClickable = false;
//            editText.SetTextIsSelectable(false);
//            editText.SetOnLongClickListener(new NoLongClickListener());
//            editText.SetOnTouchListener(new NoTouchListener());
//        }
//    }

//    protected override void OnDetached()
//    {
//        if (Control is EditText editText)
//        {
//            if (OperatingSystem.IsAndroidVersionAtLeast(23))
//            {
//                editText.CustomInsertionActionModeCallback = null;
//                editText.CustomSelectionActionModeCallback = null;
//            }
//            editText.LongClickable = true;
//            editText.SetTextIsSelectable(true);
//            editText.SetOnLongClickListener(null);
//            editText.SetOnTouchListener(null);
//        }
//    }
//}

//// Les écouteurs DOIVENT être public et externes à la classe principale !
//public class NoPasteActionModeCallback : Java.Lang.Object, ActionMode.ICallback
//{
//    public bool OnCreateActionMode(ActionMode? mode, IMenu? menu) => false;
//    public bool OnPrepareActionMode(ActionMode? mode, IMenu? menu) => false;
//    public bool OnActionItemClicked(ActionMode? mode, IMenuItem? item) => false;
//    public void OnDestroyActionMode(ActionMode? mode) { }
//}

//public class NoLongClickListener : Java.Lang.Object, Android.Views.View.IOnLongClickListener
//{
//    public bool OnLongClick(Android.Views.View? v) => true;
//}

//public class NoTouchListener : Java.Lang.Object, Android.Views.View.IOnTouchListener
//{
//    public bool OnTouch(Android.Views.View? v, MotionEvent? e)
//    {
//        if (e != null && e.Action == MotionEventActions.Down && v is EditText editText)
//        {
//            editText.SetSelection(editText.Text?.Length ?? 0);
//            return true;
//        }
//        return false;
//    }
//}