//namespace Minmaxdev.Data.Handling.Service
//{
//    public abstract class CachedDataManager<TModel, TProperty>
//    {
//        private TProperty GetWhenLoaded(TModel model) => CachedData(model).Data;

//        private bool IsLoaded(TModel model) => CachedData(model).IsLoaded;

//        private void SetData(TModel model, TProperty value) => CachedData(model).SetData(value);

//        protected abstract CachedData<TProperty> CachedData(TModel model);

//        protected abstract TProperty Load(TModel model);

//        public TProperty Get(TModel model)
//        {
//            if (IsLoaded(model) == false)
//                SetData(model, Load(model));

//            return GetWhenLoaded(model);
//        }
//    }
//}