(function webpackUniversalModuleDefinition(e, t) {
    "object" == typeof exports && "object" == typeof module ? module.exports = t() : "function" == typeof define && define.amd ? define([], t) : "object" == typeof exports ? exports["accessibleAutocomplete"] = t() : e["accessibleAutocomplete"] = t()
})(window, function() {
    return function(n) {
        var r = {};

        function o(e) {
            if (r[e]) return r[e].exports;
            var t = r[e] = {
                i: e,
                l: !1,
                exports: {}
            };
            return n[e].call(t.exports, t, t.exports, o), t.l = !0, t.exports
        }
        return o.m = n, o.c = r, o.d = function(e, t, n) {
            o.o(e, t) || Object.defineProperty(e, t, {
                enumerable: !0,
                get: n
            })
        }, o.r = function(e) {
            "undefined" != typeof Symbol && Symbol.toStringTag && Object.defineProperty(e, Symbol.toStringTag, {
                value: "Module"
            }), Object.defineProperty(e, "__esModule", {
                value: !0
            })
        }, o.t = function(t, e) {
            if (1 & e && (t = o(t)), 8 & e) return t;
            if (4 & e && "object" == typeof t && t && t.__esModule) return t;
            var n = Object.create(null);
            if (o.r(n), Object.defineProperty(n, "default", {
                    enumerable: !0,
                    value: t
                }), 2 & e && "string" != typeof t)
                for (var r in t) o.d(n, r, function(e) {
                    return t[e]
                }.bind(null, r));
            return n
        }, o.n = function(e) {
            var t = e && e.__esModule ? function() {
                return e["default"]
            } : function() {
                return e
            };
            return o.d(t, "a", t), t
        }, o.o = function(e, t) {
            return Object.prototype.hasOwnProperty.call(e, t)
        }, o.p = "/", o(o.s = 37)
    }([function(e, t, n) {
        var m = n(1),
            v = n(6),
            y = n(7),
            b = n(16),
            _ = n(18),
            g = "prototype",
            w = function(e, t, n) {
                var r, o, i, u, a = e & w.F,
                    l = e & w.G,
                    s = e & w.S,
                    c = e & w.P,
                    p = e & w.B,
                    f = l ? m : s ? m[t] || (m[t] = {}) : (m[t] || {})[g],
                    d = l ? v : v[t] || (v[t] = {}),
                    h = d[g] || (d[g] = {});
                for (r in l && (n = t), n) i = ((o = !a && f && f[r] !== undefined) ? f : n)[r], u = p && o ? _(i, m) : c && "function" == typeof i ? _(Function.call, i) : i, f && b(f, r, i, e & w.U), d[r] != i && y(d, r, u), c && h[r] != i && (h[r] = i)
            };
        m.core = v, w.F = 1, w.G = 2, w.S = 4, w.P = 8, w.B = 16, w.W = 32, w.U = 64, w.R = 128, e.exports = w
    }, function(e, t) {
        var n = e.exports = "undefined" != typeof window && window.Math == Math ? window : "undefined" != typeof self && self.Math == Math ? self : Function("return this")();
        "number" == typeof __g && (__g = n)
    }, function(e, t) {
        e.exports = function(e) {
            return "object" == typeof e ? null !== e : "function" == typeof e
        }
    }, function(e, t, n) {
        e.exports = !n(4)(function() {
            return 7 != Object.defineProperty({}, "a", {
                get: function() {
                    return 7
                }
            }).a
        })
    }, function(e, t) {
        e.exports = function(e) {
            try {
                return !!e()
            } catch (t) {
                return !0
            }
        }
    }, function(e, t, n) {
        "use strict";
        n.r(t), n.d(t, "h", function() {
            return r
        }), n.d(t, "createElement", function() {
            return r
        }), n.d(t, "cloneElement", function() {
            return i
        }), n.d(t, "Component", function() {
            return b
        }), n.d(t, "render", function() {
            return _
        }), n.d(t, "rerender", function() {
            return f
        }), n.d(t, "options", function() {
            return E
        });
        var l = function l() {},
            E = {},
            s = [],
            c = [];

        function r(e, t) {
            var n, r, o, i, u = c;
            for (i = arguments.length; 2 < i--;) s.push(arguments[i]);
            for (t && null != t.children && (s.length || s.push(t.children), delete t.children); s.length;)
                if ((r = s.pop()) && r.pop !== undefined)
                    for (i = r.length; i--;) s.push(r[i]);
                else "boolean" == typeof r && (r = null), (o = "function" != typeof e) && (null == r ? r = "" : "number" == typeof r ? r = String(r) : "string" != typeof r && (o = !1)), o && n ? u[u.length - 1] += r : u === c ? u = [r] : u.push(r), n = o;
            var a = new l;
            return a.nodeName = e, a.children = u, a.attributes = null == t ? undefined : t, a.key = null == t ? undefined : t.key, E.vnode !== undefined && E.vnode(a), a
        }

        function N(e, t) {
            for (var n in t) e[n] = t[n];
            return e
        }
        var o = "function" == typeof Promise ? Promise.resolve().then.bind(Promise.resolve()) : setTimeout;

        function i(e, t) {
            return r(e.nodeName, N(N({}, e.attributes), t), 2 < arguments.length ? [].slice.call(arguments, 2) : e.children)
        }
        var p = /acit|ex(?:s|g|n|p|$)|rph|ows|mnc|ntw|ine[ch]|zoo|^ord/i,
            u = [];

        function a(e) {
            !e._dirty && (e._dirty = !0) && 1 == u.push(e) && (E.debounceRendering || o)(f)
        }

        function f() {
            var e, t = u;
            for (u = []; e = t.pop();) e._dirty && V(e)
        }

        function I(e, t) {
            return e.normalizedNodeName === t || e.nodeName.toLowerCase() === t.toLowerCase()
        }

        function M(e) {
            var t = N({}, e.attributes);
            t.children = e.children;
            var n = e.nodeName.defaultProps;
            if (n !== undefined)
                for (var r in n) t[r] === undefined && (t[r] = n[r]);
            return t
        }

        function k(e) {
            var t = e.parentNode;
            t && t.removeChild(e)
        }

        function v(e, t, n, r, o) {
            if ("className" === t && (t = "class"), "key" === t);
            else if ("ref" === t) n && n(null), r && r(e);
            else if ("class" !== t || o)
                if ("style" === t) {
                    if (r && "string" != typeof r && "string" != typeof n || (e.style.cssText = r || ""), r && "object" == typeof r) {
                        if ("string" != typeof n)
                            for (var i in n) i in r || (e.style[i] = "");
                        for (var i in r) e.style[i] = "number" == typeof r[i] && !1 === p.test(i) ? r[i] + "px" : r[i]
                    }
                } else if ("dangerouslySetInnerHTML" === t) r && (e.innerHTML = r.__html || "");
            else if ("o" == t[0] && "n" == t[1]) {
                var u = t !== (t = t.replace(/Capture$/, ""));
                t = t.toLowerCase().substring(2), r ? n || e.addEventListener(t, d, u) : e.removeEventListener(t, d, u), (e._listeners || (e._listeners = {}))[t] = r
            } else if ("list" !== t && "type" !== t && !o && t in e) {
                try {
                    e[t] = null == r ? "" : r
                } catch (l) {}
                null != r && !1 !== r || "spellcheck" == t || e.removeAttribute(t)
            } else {
                var a = o && t !== (t = t.replace(/^xlink:?/, ""));
                null == r || !1 === r ? a ? e.removeAttributeNS("http://www.w3.org/1999/xlink", t.toLowerCase()) : e.removeAttribute(t) : "function" != typeof r && (a ? e.setAttributeNS("http://www.w3.org/1999/xlink", t.toLowerCase(), r) : e.setAttribute(t, r))
            } else e.className = r || ""
        }

        function d(e) {
            return this._listeners[e.type](E.event && E.event(e) || e)
        }
        var A = [],
            P = 0,
            j = !1,
            T = !1;

        function L() {
            for (var e; e = A.pop();) E.afterMount && E.afterMount(e), e.componentDidMount && e.componentDidMount()
        }

        function B(e, t, n, r, o, i) {
            P++ || (j = null != o && o.ownerSVGElement !== undefined, T = null != e && !("__preactattr_" in e));
            var u = F(e, t, n, r, i);
            return o && u.parentNode !== o && o.appendChild(u), --P || (T = !1, i || L()), u
        }

        function F(e, t, n, r, o) {
            var i = e,
                u = j;
            if (null != t && "boolean" != typeof t || (t = ""), "string" == typeof t || "number" == typeof t) return e && e.splitText !== undefined && e.parentNode && (!e._component || o) ? e.nodeValue != t && (e.nodeValue = t) : (i = document.createTextNode(t), e && (e.parentNode && e.parentNode.replaceChild(i, e), R(e, !0))), i["__preactattr_"] = !0, i;
            var a = t.nodeName;
            if ("function" == typeof a) return function d(e, t, n, r) {
                var o = e && e._component,
                    i = o,
                    u = e,
                    a = o && e._componentConstructor === t.nodeName,
                    l = a,
                    s = M(t);
                for (; o && !l && (o = o._parentComponent);) l = o.constructor === t.nodeName;
                o && l && (!r || o._component) ? (U(o, s, 3, n, r), e = o.base) : (i && !a && (q(i), e = u = null), o = D(t.nodeName, s, n), e && !o.nextBase && (o.nextBase = e, u = null), U(o, s, 1, n, r), e = o.base, u && e !== u && (u._component = null, R(u, !1)));
                return e
            }(e, t, n, r);
            if (j = "svg" === a || "foreignObject" !== a && j, a = String(a), (!e || !I(e, a)) && (i = function h(e, t) {
                    var n = t ? document.createElementNS("http://www.w3.org/2000/svg", e) : document.createElement(e);
                    return n.normalizedNodeName = e, n
                }(a, j), e)) {
                for (; e.firstChild;) i.appendChild(e.firstChild);
                e.parentNode && e.parentNode.replaceChild(i, e), R(e, !0)
            }
            var l = i.firstChild,
                s = i["__preactattr_"],
                c = t.children;
            if (null == s) {
                s = i["__preactattr_"] = {};
                for (var p = i.attributes, f = p.length; f--;) s[p[f].name] = p[f].value
            }
            return !T && c && 1 === c.length && "string" == typeof c[0] && null != l && l.splitText !== undefined && null == l.nextSibling ? l.nodeValue != c[0] && (l.nodeValue = c[0]) : (c && c.length || null != l) && function S(e, t, n, r, o) {
                    var i, u, a, l, s, c = e.childNodes,
                        p = [],
                        f = {},
                        d = 0,
                        h = 0,
                        m = c.length,
                        v = 0,
                        y = t ? t.length : 0;
                    if (0 !== m)
                        for (var b = 0; b < m; b++) {
                            var _ = c[b],
                                g = _["__preactattr_"],
                                w = y && g ? _._component ? _._component.__key : g.key : null;
                            null != w ? (d++, f[w] = _) : (g || (_.splitText !== undefined ? !o || _.nodeValue.trim() : o)) && (p[v++] = _)
                        }
                    if (0 !== y)
                        for (var b = 0; b < y; b++) {
                            l = t[b], s = null;
                            var w = l.key;
                            if (null != w) d && f[w] !== undefined && (s = f[w], f[w] = undefined, d--);
                            else if (h < v)
                                for (i = h; i < v; i++)
                                    if (p[i] !== undefined && (x = u = p[i], C = o, "string" == typeof(O = l) || "number" == typeof O ? x.splitText !== undefined : "string" == typeof O.nodeName ? !x._componentConstructor && I(x, O.nodeName) : C || x._componentConstructor === O.nodeName)) {
                                        s = u, p[i] = undefined, i === v - 1 && v--, i === h && h++;
                                        break
                                    }
                            s = F(s, l, n, r), a = c[b], s && s !== e && s !== a && (null == a ? e.appendChild(s) : s === a.nextSibling ? k(a) : e.insertBefore(s, a))
                        }
                    var x, O, C;
                    if (d)
                        for (var b in f) f[b] !== undefined && R(f[b], !1);
                    for (; h <= v;)(s = p[v--]) !== undefined && R(s, !1)
                }(i, c, n, r, T || null != s.dangerouslySetInnerHTML),
                function m(e, t, n) {
                    var r;
                    for (r in n) t && null != t[r] || null == n[r] || v(e, r, n[r], n[r] = undefined, j);
                    for (r in t) "children" === r || "innerHTML" === r || r in n && t[r] === ("value" === r || "checked" === r ? e[r] : n[r]) || v(e, r, n[r], n[r] = t[r], j)
                }(i, t.attributes, s), j = u, i
        }

        function R(e, t) {
            var n = e._component;
            n ? q(n) : (null != e["__preactattr_"] && e["__preactattr_"].ref && e["__preactattr_"].ref(null), !1 !== t && null != e["__preactattr_"] || k(e), h(e))
        }

        function h(e) {
            for (e = e.lastChild; e;) {
                var t = e.previousSibling;
                R(e, !0), e = t
            }
        }
        var m = [];

        function D(e, t, n) {
            var r, o = m.length;
            for (e.prototype && e.prototype.render ? (r = new e(t, n), b.call(r, t, n)) : ((r = new b(t, n)).constructor = e, r.render = y); o--;)
                if (m[o].constructor === e) return r.nextBase = m[o].nextBase, m.splice(o, 1), r;
            return r
        }

        function y(e, t, n) {
            return this.constructor(e, n)
        }

        function U(e, t, n, r, o) {
            e._disable || (e._disable = !0, e.__ref = t.ref, e.__key = t.key, delete t.ref, delete t.key, "undefined" == typeof e.constructor.getDerivedStateFromProps && (!e.base || o ? e.componentWillMount && e.componentWillMount() : e.componentWillReceiveProps && e.componentWillReceiveProps(t, r)), r && r !== e.context && (e.prevContext || (e.prevContext = e.context), e.context = r), e.prevProps || (e.prevProps = e.props), e.props = t, e._disable = !1, 0 !== n && (1 !== n && !1 === E.syncComponentUpdates && e.base ? a(e) : V(e, 1, o)), e.__ref && e.__ref(e))
        }

        function V(e, t, n, r) {
            if (!e._disable) {
                var o, i, u, a = e.props,
                    l = e.state,
                    s = e.context,
                    c = e.prevProps || a,
                    p = e.prevState || l,
                    f = e.prevContext || s,
                    d = e.base,
                    h = e.nextBase,
                    m = d || h,
                    v = e._component,
                    y = !1,
                    b = f;
                if (e.constructor.getDerivedStateFromProps && (l = N(N({}, l), e.constructor.getDerivedStateFromProps(a, l)), e.state = l), d && (e.props = c, e.state = p, e.context = f, 2 !== t && e.shouldComponentUpdate && !1 === e.shouldComponentUpdate(a, l, s) ? y = !0 : e.componentWillUpdate && e.componentWillUpdate(a, l, s), e.props = a, e.state = l, e.context = s), e.prevProps = e.prevState = e.prevContext = e.nextBase = null, e._dirty = !1, !y) {
                    o = e.render(a, l, s), e.getChildContext && (s = N(N({}, s), e.getChildContext())), d && e.getSnapshotBeforeUpdate && (b = e.getSnapshotBeforeUpdate(c, p));
                    var _, g, w = o && o.nodeName;
                    if ("function" == typeof w) {
                        var x = M(o);
                        (i = v) && i.constructor === w && x.key == i.__key ? U(i, x, 1, s, !1) : (_ = i, e._component = i = D(w, x, s), i.nextBase = i.nextBase || h, i._parentComponent = e, U(i, x, 0, s, !1), V(i, 1, n, !0)), g = i.base
                    } else u = m, (_ = v) && (u = e._component = null), (m || 1 === t) && (u && (u._component = null), g = function B(t, n, r, o, i, u) {
                        P++ || (j = null != i && i.ownerSVGElement !== undefined, T = null != t && !("__preactattr_" in t));
                        var a = F(t, n, r, o, u);
                        return i && a.parentNode !== i && i.appendChild(a), --P || (T = !1, u || L()), a
                    }(u, o, s, n || !d, m && m.parentNode, !0));
                    if (m && g !== m && i !== v) {
                        var O = m.parentNode;
                        O && g !== O && (O.replaceChild(g, m), _ || (m._component = null, R(m, !1)))
                    }
                    if (_ && q(_), (e.base = g) && !r) {
                        for (var C = e, S = e; S = S._parentComponent;)(C = S).base = g;
                        g._component = C, g._componentConstructor = C.constructor
                    }
                }
                for (!d || n ? A.unshift(e) : y || (e.componentDidUpdate && e.componentDidUpdate(c, p, b), E.afterUpdate && E.afterUpdate(e)); e._renderCallbacks.length;) e._renderCallbacks.pop().call(e);
                P || r || L()
            }
        }

        function q(e) {
            E.beforeUnmount && E.beforeUnmount(e);
            var t = e.base;
            e._disable = !0, e.componentWillUnmount && e.componentWillUnmount(), e.base = null;
            var n = e._component;
            n ? q(n) : t && (t["__preactattr_"] && t["__preactattr_"].ref && t["__preactattr_"].ref(null), k(e.nextBase = t), m.push(e), h(t)), e.__ref && e.__ref(null)
        }

        function b(e, t) {
            this._dirty = !0, this.context = t, this.props = e, this.state = this.state || {}, this._renderCallbacks = []
        }

        function _(e, t, n) {
            return B(n, e, {}, !1, t, !1)
        }
        N(b.prototype, {
            setState: function(e, t) {
                this.prevState || (this.prevState = this.state), this.state = N(N({}, this.state), "function" == typeof e ? e(this.state, this.props) : e), t && this._renderCallbacks.push(t), a(this)
            },
            forceUpdate: function(e) {
                e && this._renderCallbacks.push(e), V(this, 2)
            },
            render: function _() {}
        });
        var g = {
            h: r,
            createElement: r,
            cloneElement: i,
            Component: b,
            render: _,
            rerender: f,
            options: E
        };
        t["default"] = g
    }, function(e, t) {
        var n = e.exports = {
            version: "2.5.7"
        };
        "number" == typeof __e && (__e = n)
    }, function(e, t, n) {
        var r = n(8),
            o = n(40);
        e.exports = n(3) ? function(e, t, n) {
            return r.f(e, t, o(1, n))
        } : function(e, t, n) {
            return e[t] = n, e
        }
    }, function(e, t, n) {
        var o = n(9),
            i = n(38),
            u = n(39),
            a = Object.defineProperty;
        t.f = n(3) ? Object.defineProperty : function(e, t, n) {
            if (o(e), t = u(t, !0), o(n), i) try {
                return a(e, t, n)
            } catch (r) {}
            if ("get" in n || "set" in n) throw TypeError("Accessors not supported!");
            return "value" in n && (e[t] = n.value), e
        }
    }, function(e, t, n) {
        var r = n(2);
        e.exports = function(e) {
            if (!r(e)) throw TypeError(e + " is not an object!");
            return e
        }
    }, function(e, t) {
        var n = 0,
            r = Math.random();
        e.exports = function(e) {
            return "Symbol(".concat(e === undefined ? "" : e, ")_", (++n + r).toString(36))
        }
    }, function(e, t, n) {
        var r = n(22);
        e.exports = Object("z").propertyIsEnumerable(0) ? Object : function(e) {
            return "String" == r(e) ? e.split("") : Object(e)
        }
    }, function(e, t) {
        e.exports = function(e) {
            if (e == undefined) throw TypeError("Can't call method on  " + e);
            return e
        }
    }, function(e, t, n) {
        "use strict";
        var r = n(4);
        e.exports = function(e, t) {
            return !!e && r(function() {
                t ? e.call(null, function() {}, 1) : e.call(null)
            })
        }
    }, function(e, t, n) {
        var r = n(0);
        r(r.S + r.F, "Object", {
            assign: n(41)
        })
    }, function(e, t, n) {
        var r = n(2),
            o = n(1).document,
            i = r(o) && r(o.createElement);
        e.exports = function(e) {
            return i ? o.createElement(e) : {}
        }
    }, function(e, t, n) {
        var i = n(1),
            u = n(7),
            a = n(17),
            l = n(10)("src"),
            r = "toString",
            o = Function[r],
            s = ("" + o).split(r);
        n(6).inspectSource = function(e) {
            return o.call(e)
        }, (e.exports = function(e, t, n, r) {
            var o = "function" == typeof n;
            o && (a(n, "name") || u(n, "name", t)), e[t] !== n && (o && (a(n, l) || u(n, l, e[t] ? "" + e[t] : s.join(String(t)))), e === i ? e[t] = n : r ? e[t] ? e[t] = n : u(e, t, n) : (delete e[t], u(e, t, n)))
        })(Function.prototype, r, function() {
            return "function" == typeof this && this[l] || o.call(this)
        })
    }, function(e, t) {
        var n = {}.hasOwnProperty;
        e.exports = function(e, t) {
            return n.call(e, t)
        }
    }, function(e, t, n) {
        var i = n(19);
        e.exports = function(r, o, e) {
            if (i(r), o === undefined) return r;
            switch (e) {
                case 1:
                    return function(e) {
                        return r.call(o, e)
                    };
                case 2:
                    return function(e, t) {
                        return r.call(o, e, t)
                    };
                case 3:
                    return function(e, t, n) {
                        return r.call(o, e, t, n)
                    }
            }
            return function() {
                return r.apply(o, arguments)
            }
        }
    }, function(e, t) {
        e.exports = function(e) {
            if ("function" != typeof e) throw TypeError(e + " is not a function!");
            return e
        }
    }, function(e, t, n) {
        var r = n(42),
            o = n(28);
        e.exports = Object.keys || function(e) {
            return r(e, o)
        }
    }, function(e, t, n) {
        var r = n(11),
            o = n(12);
        e.exports = function(e) {
            return r(o(e))
        }
    }, function(e, t) {
        var n = {}.toString;
        e.exports = function(e) {
            return n.call(e).slice(8, -1)
        }
    }, function(e, t, n) {
        var l = n(21),
            s = n(24),
            c = n(43);
        e.exports = function(a) {
            return function(e, t, n) {
                var r, o = l(e),
                    i = s(o.length),
                    u = c(n, i);
                if (a && t != t) {
                    for (; u < i;)
                        if ((r = o[u++]) != r) return !0
                } else
                    for (; u < i; u++)
                        if ((a || u in o) && o[u] === t) return a || u || 0; return !a && -1
            }
        }
    }, function(e, t, n) {
        var r = n(25),
            o = Math.min;
        e.exports = function(e) {
            return 0 < e ? o(r(e), 9007199254740991) : 0
        }
    }, function(e, t) {
        var n = Math.ceil,
            r = Math.floor;
        e.exports = function(e) {
            return isNaN(e = +e) ? 0 : (0 < e ? r : n)(e)
        }
    }, function(e, t, n) {
        var r = n(27)("keys"),
            o = n(10);
        e.exports = function(e) {
            return r[e] || (r[e] = o(e))
        }
    }, function(e, t, n) {
        var r = n(6),
            o = n(1),
            i = "__core-js_shared__",
            u = o[i] || (o[i] = {});
        (e.exports = function(e, t) {
            return u[e] || (u[e] = t !== undefined ? t : {})
        })("versions", []).push({
            version: r.version,
            mode: n(44) ? "pure" : "global",
            copyright: "© 2018 Denis Pushkarev (zloirock.ru)"
        })
    }, function(e, t) {
        e.exports = "constructor,hasOwnProperty,isPrototypeOf,propertyIsEnumerable,toLocaleString,toString,valueOf".split(",")
    }, function(e, t, n) {
        var r = n(12);
        e.exports = function(e) {
            return Object(r(e))
        }
    }, function(e, t, n) {
        var r = n(8).f,
            o = Function.prototype,
            i = /^\s*function ([^ (]*)/;
        "name" in o || n(3) && r(o, "name", {
            configurable: !0,
            get: function() {
                try {
                    return ("" + this).match(i)[1]
                } catch (e) {
                    return ""
                }
            }
        })
    }, function(e, t, n) {
        "use strict";
        var r = n(0),
            o = n(32)(1);
        r(r.P + r.F * !n(13)([].map, !0), "Array", {
            map: function(e) {
                return o(this, e, arguments[1])
            }
        })
    }, function(e, t, n) {
        var _ = n(18),
            g = n(11),
            w = n(29),
            x = n(24),
            r = n(47);
        e.exports = function(p, e) {
            var f = 1 == p,
                d = 2 == p,
                h = 3 == p,
                m = 4 == p,
                v = 6 == p,
                y = 5 == p || v,
                b = e || r;
            return function(e, t, n) {
                for (var r, o, i = w(e), u = g(i), a = _(t, n, 3), l = x(u.length), s = 0, c = f ? b(e, l) : d ? b(e, 0) : undefined; s < l; s++)
                    if ((y || s in u) && (o = a(r = u[s], s, i), p))
                        if (f) c[s] = o;
                        else if (o) switch (p) {
                    case 3:
                        return !0;
                    case 5:
                        return r;
                    case 6:
                        return s;
                    case 2:
                        c.push(r)
                } else if (m) return !1;
                return v ? -1 : h || m ? m : c
            }
        }
    }, function(e, t, n) {
        var r = n(22);
        e.exports = Array.isArray || function(e) {
            return "Array" == r(e)
        }
    }, function(e, t, n) {
        var r = n(27)("wks"),
            o = n(10),
            i = n(1).Symbol,
            u = "function" == typeof i;
        (e.exports = function(e) {
            return r[e] || (r[e] = u && i[e] || (u ? i : o)("Symbol." + e))
        }).store = r
    }, function(e, t, n) {
        "use strict";
        var r = n(0),
            o = n(23)(!1),
            i = [].indexOf,
            u = !!i && 1 / [1].indexOf(1, -0) < 0;
        r(r.P + r.F * (u || !n(13)(i)), "Array", {
            indexOf: function(e) {
                return u ? i.apply(this, arguments) || 0 : o(this, e, arguments[1])
            }
        })
    }, function(e, t, n) {
        var r = n(0);
        r(r.S, "Object", {
            create: n(52)
        })
    }, function(e, t, n) {
        "use strict";
        t.__esModule = !0, t["default"] = void 0, n(14), n(30), n(31), n(35), n(49), n(50);
        var r = n(5),
            o = function l(e) {
                return e && e.__esModule ? e : {
                    "default": e
                }
            }(n(51));

        function i(e) {
            if (!e.element) throw new Error("element is not defined");
            if (!e.id) throw new Error("id is not defined");
            if (!e.source) throw new Error("source is not defined");
            Array.isArray(e.source) && (e.source = u(e.source)), (0, r.render)((0, r.createElement)(o["default"], e), e.element)
        }
        var u = function u(n) {
            return function(t, e) {
                e(n.filter(function(e) {
                    return -1 !== e.toLowerCase().indexOf(t.toLowerCase())
                }))
            }
        };
        i.enhanceSelectElement = function(n) {
            if (!n.selectElement) throw new Error("selectElement is not defined");
            if (!n.source) {
                var e = [].filter.call(n.selectElement.options, function(e) {
                    return e.value || n.preserveNullOptions
                });
                n.source = e.map(function(e) {
                    return e.textContent || e.innerText
                })
            }
            if (n.onConfirm = n.onConfirm || function(t) {
                    var e = [].filter.call(n.selectElement.options, function(e) {
                        return (e.textContent || e.innerText) === t
                    })[0];
                    e && (e.selected = !0)
                }, n.selectElement.value || n.defaultValue === undefined) {
                var t = n.selectElement.options[n.selectElement.options.selectedIndex];
                n.defaultValue = t.textContent || t.innerText
            }
            n.name === undefined && (n.name = ""), n.id === undefined && (n.selectElement.id === undefined ? n.id = "" : n.id = n.selectElement.id), n.autoselect === undefined && (n.autoselect = !0);
            var r = document.createElement("span");
            n.selectElement.parentNode.insertBefore(r, n.selectElement), i(Object.assign({}, n, {
                element: r
            })), n.selectElement.style.display = "none", n.selectElement.id = n.selectElement.id + "-select"
        };
        var a = i;
        t["default"] = a
    }, function(e, t, n) {
        e.exports = !n(3) && !n(4)(function() {
            return 7 != Object.defineProperty(n(15)("div"), "a", {
                get: function() {
                    return 7
                }
            }).a
        })
    }, function(e, t, n) {
        var o = n(2);
        e.exports = function(e, t) {
            if (!o(e)) return e;
            var n, r;
            if (t && "function" == typeof(n = e.toString) && !o(r = n.call(e))) return r;
            if ("function" == typeof(n = e.valueOf) && !o(r = n.call(e))) return r;
            if (!t && "function" == typeof(n = e.toString) && !o(r = n.call(e))) return r;
            throw TypeError("Can't convert object to primitive value")
        }
    }, function(e, t) {
        e.exports = function(e, t) {
            return {
                enumerable: !(1 & e),
                configurable: !(2 & e),
                writable: !(4 & e),
                value: t
            }
        }
    }, function(e, t, n) {
        "use strict";
        var f = n(20),
            d = n(45),
            h = n(46),
            m = n(29),
            v = n(11),
            o = Object.assign;
        e.exports = !o || n(4)(function() {
            var e = {},
                t = {},
                n = Symbol(),
                r = "abcdefghijklmnopqrst";
            return e[n] = 7, r.split("").forEach(function(e) {
                t[e] = e
            }), 7 != o({}, e)[n] || Object.keys(o({}, t)).join("") != r
        }) ? function(e, t) {
            for (var n = m(e), r = arguments.length, o = 1, i = d.f, u = h.f; o < r;)
                for (var a, l = v(arguments[o++]), s = i ? f(l).concat(i(l)) : f(l), c = s.length, p = 0; p < c;) u.call(l, a = s[p++]) && (n[a] = l[a]);
            return n
        } : o
    }, function(e, t, n) {
        var u = n(17),
            a = n(21),
            l = n(23)(!1),
            s = n(26)("IE_PROTO");
        e.exports = function(e, t) {
            var n, r = a(e),
                o = 0,
                i = [];
            for (n in r) n != s && u(r, n) && i.push(n);
            for (; t.length > o;) u(r, n = t[o++]) && (~l(i, n) || i.push(n));
            return i
        }
    }, function(e, t, n) {
        var r = n(25),
            o = Math.max,
            i = Math.min;
        e.exports = function(e, t) {
            return (e = r(e)) < 0 ? o(e + t, 0) : i(e, t)
        }
    }, function(e, t) {
        e.exports = !1
    }, function(e, t) {
        t.f = Object.getOwnPropertySymbols
    }, function(e, t) {
        t.f = {}.propertyIsEnumerable
    }, function(e, t, n) {
        var r = n(48);
        e.exports = function(e, t) {
            return new(r(e))(t)
        }
    }, function(e, t, n) {
        var r = n(2),
            o = n(33),
            i = n(34)("species");
        e.exports = function(e) {
            var t;
            return o(e) && ("function" != typeof(t = e.constructor) || t !== Array && !o(t.prototype) || (t = undefined), r(t) && null === (t = t[i]) && (t = undefined)), t === undefined ? Array : t
        }
    }, function(e, t, n) {
        "use strict";
        var r = n(0),
            o = n(32)(2);
        r(r.P + r.F * !n(13)([].filter, !0), "Array", {
            filter: function(e) {
                return o(this, e, arguments[1])
            }
        })
    }, function(e, t, n) {
        var r = n(0);
        r(r.S, "Array", {
            isArray: n(33)
        })
    }, function(e, t, n) {
        "use strict";
        t.__esModule = !0, t["default"] = void 0, n(14), n(36), n(30), n(31), n(35), n(55), n(58);
        var J = n(5),
            X = o(n(60)),
            r = o(n(61));

        function o(e) {
            return e && e.__esModule ? e : {
                "default": e
            }
        }

        function Y() {
            return (Y = Object.assign || function(e) {
                for (var t = 1; t < arguments.length; t++) {
                    var n = arguments[t];
                    for (var r in n) Object.prototype.hasOwnProperty.call(n, r) && (e[r] = n[r])
                }
                return e
            }).apply(this, arguments)
        }

        function i(e) {
            if (void 0 === e) throw new ReferenceError("this hasn't been initialised - super() hasn't been called");
            return e
        }
        var u, a = {
                13: "enter",
                27: "escape",
                32: "space",
                38: "up",
                40: "down"
            },
            Z = ((u = document.createElement("x")).style.cssText = "pointer-events:auto", "auto" === u.style.pointerEvents);

        function ee() {
            return !(!navigator.userAgent.match(/(iPod|iPhone|iPad)/g) || !navigator.userAgent.match(/AppleWebKit/g))
        }
        var l = function(n) {
            function e(e) {
                var t;
                return (t = n.call(this, e) || this).elementReferences = {}, t.state = {
                    focused: null,
                    hovered: null,
                    clicked: null,
                    menuOpen: !1,
                    options: e.defaultValue ? [e.defaultValue] : [],
                    query: e.defaultValue,
                    validChoiceMade: !1,
                    selected: null,
                    ariaHint: !0
                }, t.handleComponentBlur = t.handleComponentBlur.bind(i(i(t))), t.handleKeyDown = t.handleKeyDown.bind(i(i(t))), t.handleUpArrow = t.handleUpArrow.bind(i(i(t))), t.handleDownArrow = t.handleDownArrow.bind(i(i(t))), t.handleEnter = t.handleEnter.bind(i(i(t))), t.handlePrintableKey = t.handlePrintableKey.bind(i(i(t))), t.handleListMouseLeave = t.handleListMouseLeave.bind(i(i(t))), t.handleOptionBlur = t.handleOptionBlur.bind(i(i(t))), t.handleOptionClick = t.handleOptionClick.bind(i(i(t))), t.handleOptionFocus = t.handleOptionFocus.bind(i(i(t))), t.handleOptionMouseEnter = t.handleOptionMouseEnter.bind(i(i(t))), t.handleInputBlur = t.handleInputBlur.bind(i(i(t))), t.handleInputChange = t.handleInputChange.bind(i(i(t))), t.handleInputFocus = t.handleInputFocus.bind(i(i(t))), t.pollInputElement = t.pollInputElement.bind(i(i(t))), t.getDirectInputChanges = t.getDirectInputChanges.bind(i(i(t))), t
            }(function r(e, t) {
                e.prototype = Object.create(t.prototype), (e.prototype.constructor = e).__proto__ = t
            })(e, n);
            var t = e.prototype;
            return t.isQueryAnOption = function(e, t) {
                var n = this;
                return -1 !== t.map(function(e) {
                    return n.templateInputValue(e).toLowerCase()
                }).indexOf(e.toLowerCase())
            }, t.componentDidMount = function() {
                this.pollInputElement()
            }, t.componentWillUnmount = function() {
                clearTimeout(this.$pollInput), clearTimeout(this.$blurInput)
            }, t.pollInputElement = function() {
                var e = this;
                this.getDirectInputChanges(), this.$pollInput = setTimeout(function() {
                    e.pollInputElement()
                }, 100)
            }, t.getDirectInputChanges = function() {
                var e = this.elementReferences[-1];
                e && e.value !== this.state.query && this.handleInputChange({
                    target: {
                        value: e.value
                    }
                })
            }, t.componentDidUpdate = function(e, t) {
                var n = this.state,
                    r = n.focused,
                    o = n.clicked,
                    i = null === r,
                    u = t.focused !== r;
                (u && !i || null !== o) && this.elementReferences[r].focus();
                var a = -1 === r,
                    l = u && null === t.focused;
                if (a && l) {
                    var s = this.elementReferences[r];
                    s.setSelectionRange(0, s.value.length)
                }
            }, t.hasAutoselect = function() {
                return !ee() && this.props.autoselect
            }, t.templateInputValue = function(e) {
                var t = this.props.templates && this.props.templates.inputValue;
                return t ? t(e) : e
            }, t.templateSuggestion = function(e) {
                var t = this.props.templates && this.props.templates.suggestion;
                return t ? t(e) : e
            }, t.handleComponentBlur = function(e) {
                var t, n = this.state,
                    r = n.options,
                    o = n.query,
                    i = n.selected;
                this.props.confirmOnBlur ? (t = e.query || o, this.props.onConfirm(r[i])) : t = o, this.setState({
                    focused: null,
                    clicked: null,
                    menuOpen: e.menuOpen || !1,
                    query: t,
                    selected: null,
                    validChoiceMade: this.isQueryAnOption(t, r)
                })
            }, t.handleListMouseLeave = function(e) {
                this.setState({
                    hovered: null
                })
            }, t.handleOptionBlur = function(e, t) {
                var n = this.state,
                    r = n.focused,
                    o = n.clicked,
                    i = n.menuOpen,
                    u = n.options,
                    a = n.selected,
                    l = null === e.relatedTarget && null === o,
                    s = e.relatedTarget === this.elementReferences[-1],
                    c = r !== t && -1 !== r;
                if (!c && l || !(c || s)) {
                    var p = i && ee();
                    this.handleComponentBlur({
                        menuOpen: p,
                        query: this.templateInputValue(u[a])
                    })
                }
            }, t.handleInputBlur = function(e) {
                var t = this,
                    n = this.state,
                    r = n.focused,
                    o = n.menuOpen,
                    i = n.options,
                    u = n.query,
                    a = n.selected,
                    l = -1 !== r;
                if (clearTimeout(this.$blurInput), !l) {
                    var s = o && ee(),
                        c = ee() ? u : this.templateInputValue(i[a]);
                    this.$blurInput = setTimeout(function() {
                        return t.handleComponentBlur({
                            menuOpen: s,
                            query: c
                        })
                    }, 200)
                }
            }, t.handleInputChange = function(e) {
                var n = this,
                    t = this.props,
                    r = t.minLength,
                    o = t.source,
                    i = t.showAllValues,
                    u = this.hasAutoselect(),
                    a = e.target.value,
                    l = 0 === a.length,
                    s = this.state.query.length !== a.length,
                    c = a.length >= r;
                this.setState({
                    query: a,
                    ariaHint: l
                }), i || !l && s && c ? o(a, function(e) {
                    var t = 0 < e.length;
                    n.setState({
                        menuOpen: t,
                        options: e,
                        selected: u && t ? 0 : -1,
                        validChoiceMade: !1
                    })
                }) : !l && c || this.setState({
                    menuOpen: !1,
                    options: []
                })
            }, t.handleInputClick = function(e) {
                this.handleInputChange(e)
            }, t.handleInputFocus = function(e) {
                var t = this.state,
                    n = t.query,
                    r = t.validChoiceMade,
                    o = t.options,
                    i = this.props.minLength,
                    u = !r && n.length >= i && 0 < o.length;
                u ? this.setState(function(e) {
                    var t = e.menuOpen;
                    return {
                        focused: -1,
                        menuOpen: u || t,
                        selected: -1
                    }
                }) : this.setState({
                    focused: -1
                })
            }, t.handleOptionFocus = function(e) {
                this.setState({
                    focused: e,
                    hovered: null,
                    selected: e
                })
            }, t.handleOptionMouseEnter = function(e, t) {
                ee() || this.setState({
                    hovered: t
                })
            }, t.handleOptionClick = function(e, t) {
                var n = this.state.options[t],
                    r = this.templateInputValue(n);
                clearTimeout(this.$blurInput), this.props.onConfirm(n), this.setState({
                    focused: -1,
                    clicked: t,
                    hovered: null,
                    menuOpen: !1,
                    query: r,
                    selected: -1,
                    validChoiceMade: !0
                }), this.forceUpdate()
            }, t.handleUpArrow = function(e) {
                e.preventDefault();
                var t = this.state,
                    n = t.menuOpen,
                    r = t.selected; - 1 !== r && n && this.handleOptionFocus(r - 1)
            }, t.handleDownArrow = function(e) {
                var t = this;
                if (e.preventDefault(), this.props.showAllValues && !1 === this.state.menuOpen) e.preventDefault(), this.props.source("", function(e) {
                    t.setState({
                        menuOpen: !0,
                        options: e,
                        selected: 0,
                        focused: 0,
                        hovered: null
                    })
                });
                else if (!0 === this.state.menuOpen) {
                    var n = this.state,
                        r = n.menuOpen,
                        o = n.options,
                        i = n.selected;
                    i !== o.length - 1 && r && this.handleOptionFocus(i + 1)
                }
            }, t.handleSpace = function(e) {
                var t = this;
                this.props.showAllValues && !1 === this.state.menuOpen && "" === this.state.query && (e.preventDefault(), this.props.source("", function(e) {
                    t.setState({
                        menuOpen: !0,
                        options: e
                    })
                })), -1 !== this.state.focused && (e.preventDefault(), this.handleOptionClick(e, this.state.focused))
            }, t.handleEnter = function(e) {
                this.state.menuOpen && (e.preventDefault(), 0 <= this.state.selected && this.handleOptionClick(e, this.state.selected))
            }, t.handlePrintableKey = function(e) {
                var t = this.elementReferences[-1];
                e.target === t || t.focus()
            }, t.handleKeyDown = function(e) {
                switch (a[e.keyCode]) {
                    case "up":
                        this.handleUpArrow(e);
                        break;
                    case "down":
                        this.handleDownArrow(e);
                        break;
                    case "space":
                        this.handleSpace(e);
                        break;
                    case "enter":
                        this.handleEnter(e);
                        break;
                    case "escape":
                        this.handleComponentBlur({
                            query: this.state.query
                        });
                        break;
                    default:
                        (function t(e) {
                            return 47 < e && e < 58 || 32 === e || 8 === e || 64 < e && e < 91 || 95 < e && e < 112 || 185 < e && e < 193 || 218 < e && e < 223
                        })(e.keyCode) && this.handlePrintableKey(e)
                }
            }, t.render = function() {
                var e, i = this,
                    t = this.props,
                    n = t.cssNamespace,
                    r = t.displayMenu,
                    u = t.id,
                    o = t.minLength,
                    a = t.name,
                    l = t.placeholder,
                    s = t.required,
                    c = t.showAllValues,
                    p = t.tNoResults,
                    f = t.tStatusQueryTooShort,
                    d = t.tStatusNoResults,
                    h = t.tStatusSelectedOption,
                    m = t.tStatusResults,
                    v = t.tAssistiveHint,
                    y = t.dropdownArrow,
                    b = this.state,
                    _ = b.focused,
                    g = b.hovered,
                    w = b.menuOpen,
                    x = b.options,
                    O = b.query,
                    C = b.selected,
                    S = b.ariaHint,
                    E = b.validChoiceMade,
                    N = this.hasAutoselect(),
                    I = -1 === _,
                    M = 0 === x.length,
                    k = 0 !== O.length,
                    A = O.length >= o,
                    P = this.props.showNoOptionsFound && I && M && k && A,
                    j = n + "__wrapper",
                    T = n + "__input",
                    L = null !== _ ? " " + T + "--focused" : "",
                    B = this.props.showAllValues ? " " + T + "--show-all-values" : " " + T + "--default",
                    F = n + "__dropdown-arrow-down",
                    R = -1 !== _ && null !== _,
                    D = n + "__menu",
                    U = D + "--" + r,
                    V = D + "--" + (w || P ? "visible" : "hidden"),
                    q = n + "__option",
                    W = n + "__hint",
                    H = this.templateInputValue(x[C]),
                    K = H && 0 === H.toLowerCase().indexOf(O.toLowerCase()) && N ? O + H.substr(O.length) : "",
                    Q = Z && K,
                    $ = u + "__assistiveHint",
                    z = S ? {
                        "aria-describedby": $
                    } : null;
                return c && "string" == typeof(e = y({
                    className: F
                })) && (e = (0, J.createElement)("div", {
                    className: n + "__dropdown-arrow-down-wrapper",
                    dangerouslySetInnerHTML: {
                        __html: e
                    }
                })), (0, J.createElement)("div", {
                    className: j,
                    onKeyDown: this.handleKeyDown
                }, (0, J.createElement)(X["default"], {
                    id: u,
                    length: x.length,
                    queryLength: O.length,
                    minQueryLength: o,
                    selectedOption: this.templateInputValue(x[C]),
                    selectedOptionIndex: C,
                    validChoiceMade: E,
                    isInFocus: null !== this.state.focused,
                    tQueryTooShort: f,
                    tNoResults: d,
                    tSelectedOption: h,
                    tResults: m
                }), Q && (0, J.createElement)("span", null, (0, J.createElement)("input", {
                    className: W,
                    readonly: !0,
                    tabIndex: "-1",
                    value: K
                })), (0, J.createElement)("input", Y({
                    "aria-expanded": w ? "true" : "false",
                    "aria-activedescendant": !!R && u + "__option--" + _,
                    "aria-owns": u + "__listbox",
                    "aria-autocomplete": this.hasAutoselect() ? "both" : "list"
                }, z, {
                    autoComplete: "off",
                    className: "" + T + L + B,
                    id: u,
                    onClick: function(e) {
                        return i.handleInputClick(e)
                    },
                    onBlur: this.handleInputBlur
                }, function G(e) {
                    return {
                        onInput: e
                    }
                }(this.handleInputChange), {
                    onFocus: this.handleInputFocus,
                    name: a,
                    placeholder: l,
                    ref: function(e) {
                        i.elementReferences[-1] = e
                    },
                    type: "text",
                    role: "combobox",
                    required: s,
                    value: O
                })), e, (0, J.createElement)("ul", {
                    className: D + " " + U + " " + V,
                    onMouseLeave: function(e) {
                        return i.handleListMouseLeave(e)
                    },
                    id: u + "__listbox",
                    role: "listbox"
                }, x.map(function(e, t) {
                    var n = (-1 === _ ? C === t : _ === t) && null === g ? " " + q + "--focused" : "",
                        r = t % 2 ? " " + q + "--odd" : "",
                        o = ee() ? "<span id=" + u + "__option-suffix--" + t + ' style="border:0;clip:rect(0 0 0 0);height:1px;marginBottom:-1px;marginRight:-1px;overflow:hidden;padding:0;position:absolute;whiteSpace:nowrap;width:1px"> ' + (t + 1) + " of " + x.length + "</span>" : "";
                    return (0, J.createElement)("li", {
                        "aria-selected": _ === t,
                        className: "" + q + n + r,
                        dangerouslySetInnerHTML: {
                            __html: i.templateSuggestion(e) + o
                        },
                        id: u + "__option--" + t,
                        key: t,
                        onBlur: function(e) {
                            return i.handleOptionBlur(e, t)
                        },
                        onClick: function(e) {
                            return i.handleOptionClick(e, t)
                        },
                        onMouseEnter: function(e) {
                            return i.handleOptionMouseEnter(e, t)
                        },
                        ref: function(e) {
                            i.elementReferences[t] = e
                        },
                        role: "option",
                        tabIndex: "-1",
                        "aria-posinset": t + 1,
                        "aria-setsize": x.length
                    })
                }), P && (0, J.createElement)("li", {
                    className: q + " " + q + "--no-results"
                }, p())), (0, J.createElement)("span", {
                    id: $,
                    style: {
                        display: "none"
                    }
                }, v()))
            }, e
        }(J.Component);
        (t["default"] = l).defaultProps = {
            autoselect: !1,
            cssNamespace: "autocomplete",
            defaultValue: "",
            displayMenu: "inline",
            minLength: 0,
            name: "input-autocomplete",
            placeholder: "",
            onConfirm: function() {},
            confirmOnBlur: !0,
            showNoOptionsFound: !0,
            showAllValues: !1,
            required: !1,
            tNoResults: function() {
                return "No results found"
            },
            tAssistiveHint: function() {
                return "When autocomplete results are available use up and down arrows to review and enter to select.  Touch device users, explore by touch or with swipe gestures."
            },
            dropdownArrow: r["default"]
        }
    }, function(e, t, r) {
        var o = r(9),
            i = r(53),
            u = r(28),
            a = r(26)("IE_PROTO"),
            l = function() {},
            s = "prototype",
            c = function() {
                var e, t = r(15)("iframe"),
                    n = u.length;
                for (t.style.display = "none", r(54).appendChild(t), t.src = "javascript:", (e = t.contentWindow.document).open(), e.write("<script>document.F=Object<\/script>"), e.close(), c = e.F; n--;) delete c[s][u[n]];
                return c()
            };
        e.exports = Object.create || function(e, t) {
            var n;
            return null !== e ? (l[s] = o(e), n = new l, l[s] = null, n[a] = e) : n = c(), t === undefined ? n : i(n, t)
        }
    }, function(e, t, n) {
        var u = n(8),
            a = n(9),
            l = n(20);
        e.exports = n(3) ? Object.defineProperties : function(e, t) {
            a(e);
            for (var n, r = l(t), o = r.length, i = 0; i < o;) u.f(e, n = r[i++], t[n]);
            return e
        }
    }, function(e, t, n) {
        var r = n(1).document;
        e.exports = r && r.documentElement
    }, function(e, t, n) {
        var r = n(0);
        r(r.P, "Function", {
            bind: n(56)
        })
    }, function(e, t, n) {
        "use strict";
        var i = n(19),
            u = n(2),
            a = n(57),
            l = [].slice,
            s = {};
        e.exports = Function.bind || function(t) {
            var n = i(this),
                r = l.call(arguments, 1),
                o = function() {
                    var e = r.concat(l.call(arguments));
                    return this instanceof o ? function(e, t, n) {
                        if (!(t in s)) {
                            for (var r = [], o = 0; o < t; o++) r[o] = "a[" + o + "]";
                            s[t] = Function("F,a", "return new F(" + r.join(",") + ")")
                        }
                        return s[t](e, n)
                    }(n, e.length, e) : a(n, e, t)
                };
            return u(n.prototype) && (o.prototype = n.prototype), o
        }
    }, function(e, t) {
        e.exports = function(e, t, n) {
            var r = n === undefined;
            switch (t.length) {
                case 0:
                    return r ? e() : e.call(n);
                case 1:
                    return r ? e(t[0]) : e.call(n, t[0]);
                case 2:
                    return r ? e(t[0], t[1]) : e.call(n, t[0], t[1]);
                case 3:
                    return r ? e(t[0], t[1], t[2]) : e.call(n, t[0], t[1], t[2]);
                case 4:
                    return r ? e(t[0], t[1], t[2], t[3]) : e.call(n, t[0], t[1], t[2], t[3])
            }
            return e.apply(n, t)
        }
    }, function(e, t, n) {
        n(59)("match", 1, function(r, o, e) {
            return [function(e) {
                "use strict";
                var t = r(this),
                    n = e == undefined ? undefined : e[o];
                return n !== undefined ? n.call(e, t) : new RegExp(e)[o](String(t))
            }, e]
        })
    }, function(e, t, n) {
        "use strict";
        var a = n(7),
            l = n(16),
            s = n(4),
            c = n(12),
            p = n(34);
        e.exports = function(t, e, n) {
            var r = p(t),
                o = n(c, r, "" [t]),
                i = o[0],
                u = o[1];
            s(function() {
                var e = {};
                return e[r] = function() {
                    return 7
                }, 7 != "" [t](e)
            }) && (l(String.prototype, t, i), a(RegExp.prototype, r, 2 == e ? function(e, t) {
                return u.call(e, this, t)
            } : function(e) {
                return u.call(e, this)
            }))
        }
    }, function(e, t, n) {
        "use strict";
        t.__esModule = !0, t["default"] = void 0, n(36);
        var _ = n(5);
        var r = function r(o, i, u) {
                var a;
                return function() {
                    var e = this,
                        t = arguments,
                        n = function n() {
                            a = null, u || o.apply(e, t)
                        },
                        r = u && !a;
                    clearTimeout(a), a = setTimeout(n, i), r && o.apply(e, t)
                }
            },
            o = function(o) {
                function e() {
                    for (var e, t = arguments.length, n = new Array(t), r = 0; r < t; r++) n[r] = arguments[r];
                    return (e = o.call.apply(o, [this].concat(n)) || this).state = {
                        bump: !1,
                        debounced: !1
                    }, e
                }(function n(e, t) {
                    e.prototype = Object.create(t.prototype), (e.prototype.constructor = e).__proto__ = t
                })(e, o);
                var t = e.prototype;
                return t.componentWillMount = function() {
                    var e = this;
                    this.debounceStatusUpdate = r(function() {
                        if (!e.state.debounced) {
                            var t = !e.props.isInFocus || e.props.validChoiceMade;
                            e.setState(function(e) {
                                return {
                                    bump: !e.bump,
                                    debounced: !0,
                                    silenced: t
                                }
                            })
                        }
                    }, 1400)
                }, t.componentWillReceiveProps = function(e) {
                    e.queryLength;
                    this.setState({
                        debounced: !1
                    })
                }, t.render = function() {
                    var e = this.props,
                        t = e.id,
                        n = e.length,
                        r = e.queryLength,
                        o = e.minQueryLength,
                        i = e.selectedOption,
                        u = e.selectedOptionIndex,
                        a = e.tQueryTooShort,
                        l = e.tNoResults,
                        s = e.tSelectedOption,
                        c = e.tResults,
                        p = this.state,
                        f = p.bump,
                        d = p.debounced,
                        h = p.silenced,
                        m = r < o,
                        v = 0 === n,
                        y = i ? s(i, n, u) : "",
                        b = null;
                    return b = m ? a(o) : v ? l() : c(n, y), this.debounceStatusUpdate(), (0, _.createElement)("div", {
                        style: {
                            border: "0",
                            clip: "rect(0 0 0 0)",
                            height: "1px",
                            marginBottom: "-1px",
                            marginRight: "-1px",
                            overflow: "hidden",
                            padding: "0",
                            position: "absolute",
                            whiteSpace: "nowrap",
                            width: "1px"
                        }
                    }, (0, _.createElement)("div", {
                        id: t + "__status--A",
                        role: "status",
                        "aria-atomic": "true",
                        "aria-live": "polite"
                    }, !h && d && f ? b : ""), (0, _.createElement)("div", {
                        id: t + "__status--B",
                        role: "status",
                        "aria-atomic": "true",
                        "aria-live": "polite"
                    }, h || !d || f ? "" : b))
                }, e
            }(_.Component);
        (t["default"] = o).defaultProps = {
            tQueryTooShort: function(e) {
                return "Type in " + e + " or more characters for results"
            },
            tNoResults: function() {
                return "No search results"
            },
            tSelectedOption: function(e, t, n) {
                return e + " " + (n + 1) + " of " + t + " is highlighted"
            },
            tResults: function(e, t) {
                return e + " " + (1 === e ? "result" : "results") + " " + (1 === e ? "is" : "are") + " available. " + t
            }
        }
    }, function(e, t, n) {
        "use strict";
        t.__esModule = !0, t["default"] = void 0;
        var r = n(5),
            o = function i(e) {
                var t = e.className;
                return (0, r.createElement)("svg", {
                    version: "1.1",
                    xmlns: "http://www.w3.org/2000/svg",
                    className: t,
                    focusable: "false"
                }, (0, r.createElement)("g", {
                    stroke: "none",
                    fill: "none",
                    "fill-rule": "evenodd"
                }, (0, r.createElement)("polygon", {
                    fill: "#000000",
                    points: "0 0 22 0 11 17"
                })))
            };
        t["default"] = o
    }])["default"]

    
});


//# sourceMappingURL=accessible-autocomplete.js.map