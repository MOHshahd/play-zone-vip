import { useState, useEffect } from 'react';
import api from '../api';

interface MenuItem { id: number; name: string; price: number; icon: string; }
interface CartItem { item: MenuItem; quantity: number; }

export default function RoomMenu() {
  const deviceId = new URLSearchParams(window.location.search).get('deviceId');
  const [deviceName, setDeviceName] = useState('');
  const [menuItems, setMenuItems] = useState<MenuItem[]>([]);
  const [cart, setCart] = useState<CartItem[]>([]);
  const [callDesc, setCallDesc] = useState('');
  const [callSent, setCallSent] = useState(false);
  const [orderSent, setOrderSent] = useState(false);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    async function load() {
      try {
        const [settingsRes, deviceRes] = await Promise.all([
          api.get('/settings'),
          deviceId ? api.get(`/devices/${deviceId}`).catch(() => null) : null,
        ]);
        const siteSettings = settingsRes.data || {};
        if (siteSettings.menuItems) {
          try { setMenuItems(JSON.parse(siteSettings.menuItems)); }
          catch { setMenuItems([]); }
        }
        if (deviceRes?.data) {
          setDeviceName(deviceRes.data.name || '');
        }
      } catch {}
      setLoading(false);
    }
    load();
  }, [deviceId]);

  const addToCart = (item: MenuItem) => {
    setCart(prev => {
      const existing = prev.find(c => c.item.id === item.id);
      if (existing) return prev.map(c => c.item.id === item.id ? { ...c, quantity: c.quantity + 1 } : c);
      return [...prev, { item, quantity: 1 }];
    });
  };

  const removeFromCart = (itemId: number) => {
    setCart(prev => {
      const existing = prev.find(c => c.item.id === itemId);
      if (!existing) return prev;
      if (existing.quantity <= 1) return prev.filter(c => c.item.id !== itemId);
      return prev.map(c => c.item.id === itemId ? { ...c, quantity: c.quantity - 1 } : c);
    });
  };

  const totalPrice = cart.reduce((sum, c) => sum + c.item.price * c.quantity, 0);

  const sendOrder = async () => {
    if (cart.length === 0) return;
    try {
      const sessionsRes = await api.get('/sessions/active');
      const sessions: any[] = sessionsRes.data || [];
      const session = sessions.find((s: any) => s.deviceId === deviceId);
      if (session) {
        for (const c of cart) {
          await api.post(`/sessions/${session.id}/add-order`, {
            name: c.item.name,
            price: c.item.price,
            quantity: c.quantity,
          });
        }
        setOrderSent(true);
        setCart([]);
        return;
      }
    } catch {}
    // fallback: create ServiceRequest
    const items = cart.map(c => ({ name: c.item.name, price: c.item.price, quantity: c.quantity }));
    const orderText = items.map(i => `${i.name} × ${i.quantity} = ${i.price * i.quantity}ج`).join('\n');
    try {
      await api.post('/servicerequests', {
        sessionId: null,
        deviceId: deviceId || '',
        deviceName: deviceName || 'غرفة',
        requestType: 'Order',
        description: `طلب:\n${orderText}\nالإجمالي: ${totalPrice}ج`,
      });
    } catch {}
    setOrderSent(true);
    setCart([]);
  };

  const sendCallStaff = async () => {
    if (!callDesc.trim()) return;
    try {
      await api.post('/servicerequests', {
        sessionId: null,
        deviceId: deviceId || '',
        deviceName: deviceName || 'غرفة',
        requestType: 'CallStaff',
        description: callDesc,
      });
      setCallSent(true);
      setCallDesc('');
    } catch {}
  };

  if (loading) {
    return (
      <div className="min-h-screen gradient-bg flex items-center justify-center">
        <div className="text-center">
          <i className="fa-solid fa-spinner fa-spin text-4xl text-purple-400 mb-4"></i>
          <p className="text-gray-400">جاري التحميل...</p>
        </div>
      </div>
    );
  }

  return (
    <div className="min-h-screen gradient-bg text-white" style={{ fontFamily: "'Cairo', sans-serif" }}>
      <div className="max-w-lg mx-auto px-4 py-6">
        <div className="text-center mb-8">
          <div className="bg-purple-600/20 w-20 h-20 rounded-2xl flex items-center justify-center mx-auto mb-4">
            <i className="fa-solid fa-gamepad text-4xl text-purple-400"></i>
          </div>
          <h1 className="text-3xl font-bold neon-text">PlayZone</h1>
          {deviceName && <p className="text-xl text-purple-300 mt-2">{deviceName}</p>}
        </div>

        <div className="bg-yellow-600/20 border border-yellow-600/50 rounded-xl p-4 mb-6 text-center">
          <i className="fa-solid fa-info-circle text-yellow-400 ml-2"></i>
          <span className="text-yellow-300">اطلب من هنا وهنوصلك الطلب للغرفة</span>
        </div>

        <div className="mb-6">
          <h2 className="text-xl font-bold mb-4 flex items-center gap-2">
            <i className="fa-solid fa-utensils text-purple-400"></i>
            المنيو
          </h2>
          {menuItems.length === 0 ? (
            <p className="text-gray-500 text-center py-8">لا توجد أصناف متاحة حالياً</p>
          ) : (
            <div className="space-y-3">
              {menuItems.map(item => {
                const inCart = cart.find(c => c.item.id === item.id);
                return (
                  <div key={item.id} className="bg-gray-800/80 rounded-xl p-4 flex items-center justify-between">
                    <div className="flex items-center gap-3">
                      <i className={`${item.icon} text-2xl text-purple-400 w-8 text-center`}></i>
                      <div>
                        <p className="font-bold">{item.name}</p>
                        <p className="text-sm text-gray-400">{item.price} ج</p>
                      </div>
                    </div>
                    <div className="flex items-center gap-2">
                      {inCart && (
                        <>
                          <button onClick={() => removeFromCart(item.id)}
                            className="w-9 h-9 rounded-lg bg-red-600/80 hover:bg-red-600 font-bold text-lg flex items-center justify-center">
                            <i className="fa-solid fa-minus text-sm"></i>
                          </button>
                          <span className="font-bold w-6 text-center text-yellow-400">{inCart.quantity}</span>
                        </>
                      )}
                      <button onClick={() => addToCart(item)}
                        className="w-9 h-9 rounded-lg bg-purple-600 hover:bg-purple-700 font-bold text-lg flex items-center justify-center">
                        <i className="fa-solid fa-plus text-sm"></i>
                      </button>
                    </div>
                  </div>
                );
              })}
            </div>
          )}
        </div>

        {cart.length > 0 && !orderSent && (
          <div className="bg-gray-800/80 rounded-xl p-4 mb-6">
            <h3 className="font-bold mb-3 flex items-center gap-2">
              <i className="fa-solid fa-shopping-cart text-yellow-400"></i>
              طلبي ({cart.reduce((s, c) => s + c.quantity, 0)})
            </h3>
            <div className="space-y-2 mb-4">
              {cart.map(c => (
                <div key={c.item.id} className="flex items-center justify-between text-sm">
                  <span>{c.item.name} × {c.quantity}</span>
                  <span className="text-yellow-400">{c.item.price * c.quantity} ج</span>
                </div>
              ))}
            </div>
            <div className="border-t border-gray-700 pt-3 flex items-center justify-between mb-4">
              <span className="font-bold text-lg">الإجمالي</span>
              <span className="text-xl font-bold text-yellow-400">{totalPrice} ج</span>
            </div>
            <button onClick={sendOrder}
              className="w-full py-3 rounded-xl font-bold text-lg text-white"
              style={{ background: 'linear-gradient(135deg, #8b5cf6 0%, #7c3aed 100%)' }}>
              <i className="fa-solid fa-paper-plane ml-2"></i>
              إرسال الطلب
            </button>
          </div>
        )}

        {orderSent && (
          <div className="bg-green-600/20 border border-green-600/50 rounded-xl p-6 mb-6 text-center animate-slide-down">
            <i className="fa-solid fa-check-circle text-4xl text-green-400 mb-3"></i>
            <p className="text-green-400 font-bold text-lg">تم إرسال الطلب ✅</p>
            <p className="text-sm text-gray-400 mt-2">سيتم تجهيز طلبك وإيصاله للغرفة</p>
            <button onClick={() => setOrderSent(false)}
              className="mt-4 px-6 py-2 rounded-lg bg-gray-700 text-sm font-bold">
              طلب المزيد
            </button>
          </div>
        )}

        <div className="bg-gray-800/80 rounded-xl p-4 mb-6">
          <h2 className="text-xl font-bold mb-4 flex items-center gap-2">
            <i className="fa-solid fa-bell text-orange-400"></i>
            استدعاء موظف
          </h2>
          {callSent ? (
            <div className="text-center py-4">
              <i className="fa-solid fa-check-circle text-3xl text-green-400 mb-2"></i>
              <p className="text-green-400 font-bold">تم إرسال طلبك ✅</p>
              <p className="text-sm text-gray-400 mt-1">سيصلك الموظف قريباً</p>
              <button onClick={() => setCallSent(false)}
                className="mt-3 px-6 py-2 rounded-lg bg-gray-700 text-sm font-bold">
                استدعاء مرة أخرى
              </button>
            </div>
          ) : (
            <div className="space-y-3">
              <textarea
                value={callDesc}
                onChange={e => setCallDesc(e.target.value)}
                placeholder="اكتب سبب الاستدعاء (مثال: عطل في الجهاز، عايز مشروب، ...)"
                className="w-full bg-gray-700 border border-gray-600 rounded-lg px-4 py-3 text-white resize-none"
                rows={3}
                maxLength={500}
              />
              <button onClick={sendCallStaff}
                className="w-full py-3 rounded-xl font-bold text-lg text-white flex items-center justify-center gap-2"
                style={{ background: 'linear-gradient(135deg, #f59e0b 0%, #d97706 100%)' }}>
                <i className="fa-solid fa-bell"></i>
                استدعاء موظف
              </button>
            </div>
          )}
        </div>

        <div className="text-center text-gray-500 text-xs mt-8 pb-8">
          <p>PlayZone - نظام إدارة البلايستيشن</p>
        </div>
      </div>
    </div>
  );
}