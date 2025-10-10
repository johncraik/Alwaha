/**
 * Alwaha Analytics Tracking Script
 * Tracks visitor behavior and sends data to AlwahaManagement
 */

(function() {
    'use strict';

    const config = {
        apiUrl: window.alwahaAnalyticsConfig?.apiUrl || 'https://management.alwahalondon.co.uk/api/analytics/track',
        apiKey: window.alwahaAnalyticsConfig?.apiKey || '',
        sessionDuration: 30 * 60 * 1000, // 30 minutes
    };

    // Generate or retrieve session ID
    function getSessionId() {
        let sessionId = sessionStorage.getItem('alwaha_session_id');
        if (!sessionId) {
            sessionId = `${Date.now()}-${Math.random().toString(36).substr(2, 9)}`;
            sessionStorage.setItem('alwaha_session_id', sessionId);
        }
        return sessionId;
    }

    // Track event
    function track(eventData) {
        const data = {
            eventType: eventData.eventType || 'PageView',
            url: window.location.href,
            referrer: document.referrer || null,
            sessionId: getSessionId(),
            screenWidth: window.screen.width,
            screenHeight: window.screen.height,
            language: navigator.language,
            pageTitle: document.title,
            ...eventData
        };

        // Send via fetch
        const headers = {
            'Content-Type': 'application/json',
        };

        // Add API key if configured
        if (config.apiKey) {
            headers['X-API-Key'] = config.apiKey;
        }

        fetch(config.apiUrl, {
            method: 'POST',
            headers: headers,
            body: JSON.stringify(data),
            mode: 'cors'
        }).catch(err => {
            console.error('Analytics tracking failed:', err);
        });
    }

    // Track page view with duration on unload
    let pageLoadTime = Date.now();
    let tracked = false;

    // Track on page load
    track({ eventType: 'PageView' });

    // Update with duration before leaving (using fetch with keepalive for reliability)
    window.addEventListener('beforeunload', function() {
        if (tracked) return;
        tracked = true;

        const duration = Math.floor((Date.now() - pageLoadTime) / 1000); // seconds
        const data = {
            eventType: 'TimeOnPage',
            url: window.location.href,
            sessionId: getSessionId(),
            duration: duration
        };

        const headers = {
            'Content-Type': 'application/json',
        };

        // Add API key if configured
        if (config.apiKey) {
            headers['X-API-Key'] = config.apiKey;
        }

        // Use fetch with keepalive for reliability during page unload
        fetch(config.apiUrl, {
            method: 'POST',
            headers: headers,
            body: JSON.stringify(data),
            keepalive: true
        }).catch(() => {
            // Silently fail on unload
        });
    });

    // Track clicks on specific elements (optional)
    document.addEventListener('click', function(e) {
        const target = e.target.closest('[data-analytics-track]');
        if (target) {
            track({
                eventType: 'Click',
                metadata: JSON.stringify({
                    element: target.getAttribute('data-analytics-track'),
                    text: target.innerText?.substring(0, 100)
                })
            });
        }
    });

    // Expose API for custom tracking
    window.alwahaAnalytics = {
        track: track,
        trackEvent: function(eventType, metadata) {
            track({
                eventType: eventType,
                metadata: metadata ? JSON.stringify(metadata) : null
            });
        }
    };

})();
