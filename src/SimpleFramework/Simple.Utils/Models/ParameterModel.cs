using Simple.Utils.Exceptions;
using Simple.Utils.Helper;
using Simple.Utils.Models.Dto;

namespace Simple.Utils.Models
{
    /// <summary>动态参数基类，参数封装在字典中,FromQuery请求需要使用ParameterModelFromQuery 参数只支持一层，不支持多层的参数</summary>
    public class ParameterModel : BaseDto
    {
        public Guid? Id { get; set; }

        /// <summary>返回字典中指定模型的值</summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T Read<T>() where T : class, new()
        {
            var model = Activator.CreateInstance<T>();
            try
            {
                // 克隆字典
                var dict = new Dictionary<string, object>(this.AdditionalData);
                dict["id"] = Id;
                ObjectHelper.TryFromDict(model, dict);
            }
            catch (Exception ex)
            {
                throw new CustomException("模型赋值异常", "模型赋值异常", ex);
            }

            return model;
        }
    }
}