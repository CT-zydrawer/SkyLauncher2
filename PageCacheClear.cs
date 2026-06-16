using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
namespace SkyLauncher;
public partial class MainWindow : Window
{
    // 最大可以缓存的页面数量，超过这个数量就会淘汰掉最久未访问的页面
    private const int MAX_CACHED_PAGES = 3;
    private readonly Dictionary<string, UserControl> _pageCache = new Dictionary<string, UserControl>();
    private readonly List<string> _pageHistory = new List<string>();

    /// <summary>
    /// 通用的页面跳转核心方法
    /// </summary>
    public void NavigateToPage<T>(Func<T> factory) where T : UserControl
    {
        string pageKey = typeof(T).FullName;

        // 如果页面已经在缓存中
        if (_pageCache.TryGetValue(pageKey, out var existingPage))
        {
            // 把它从历史记录中移除，并重新推入队尾（更新活跃度）
            _pageHistory.Remove(pageKey);
            _pageHistory.Add(pageKey);

            ContentArea.Content = existingPage;
            return;
        }

        // 如果缓存满了，淘汰掉最早打开的页面
        if (_pageCache.Count >= MAX_CACHED_PAGES)
        {
            string oldestPageKey = _pageHistory[0];
            _pageHistory.RemoveAt(0);

            if (_pageCache.TryGetValue(oldestPageKey, out var pageToDispose))
            {
                _pageCache.Remove(oldestPageKey);
                //辅助GC，清空页面的内容和数据上下文，断开与UI的连接
                pageToDispose.Content = null;
                pageToDispose.DataContext = null;
                // 调用 IDisposable.Dispose() 来释放资源，如果页面实现了这个接口
                if (pageToDispose is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }
        }

        // 创建新页面，加入缓存和历史记录
        T newPage = factory();
        _pageCache[pageKey] = newPage;
        _pageHistory.Add(pageKey);

        ContentArea.Content = newPage;
    }
}
