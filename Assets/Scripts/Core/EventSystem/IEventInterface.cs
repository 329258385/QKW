using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Solarmax
{
    public interface IEventInterface
    {
		/**
		 * 事件响应
		 * param:
		 *		id:			事件id
		 *		pData:		事件发生时的环境
		 *		customData:	用户自定义数据
		 * return:
		 *		该事件是否处理完成(以后还会不会处理此事件)，如果完成(以后不处理了)需要返回true，否则false
		 */
		bool OnEvent(EventID id, KEvent eventData, uint customData);
	}

	public class KEventBase : IEventInterface
	{
		public virtual bool OnEvent(EventID id, KEvent eventData, uint customData)
		{
			if (Filter(id, eventData, customData))
			{
				return OnEventImp(id, eventData, customData);
			}
			return false;
		}

		/**
		* 事件响应的实现部分
		* return:
		*		该事件是否处理完成(以后还会不会处理此事件)，如果完成(以后不处理了)需要返回true，否则false
		*/
		protected virtual bool OnEventImp(EventID id, KEvent eventData, uint customData)
		{
			return true;
		}
		/**
		* 事件过滤器
		* 根据事件参数决定是否要处理该事件
		* 如果需要处理则返回true，否则false
		*/
		protected virtual bool Filter(EventID id, KEvent eventData, uint customData)
		{
			return true;
		}
	}
}
