using UnityEngine;
using UnityEngine.UI;




public class BloodHUD : RawImage
{
    private Slider          _BloodSlider;

    private Vector3         _cachePos = Vector3.zero;

    protected override void OnRectTransformDimensionsChange()
    {
        base.OnRectTransformDimensionsChange();

        //获取血条
        if (_BloodSlider == null)
            _BloodSlider = transform.parent.parent.GetComponent<Slider>();

        //获取血条的值
        if (_BloodSlider != null)
        {
            //刷新血条的显示
            float value = _BloodSlider.value;
            uvRect = new Rect(0,0,value,1);
        }
    }


    public void SetValue( float fHP )
    {
        _BloodSlider.value = fHP;
    }
}
