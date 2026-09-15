
import './bswup';
import './theme';
import './events';
import { App } from './App';
import { WebInteropApp } from './WebInteropApp';
import { Ads } from './Ads';

// Expose classes on window global
(window as any).Ads = Ads;
(window as any).App = App;
(window as any).WebInteropApp = WebInteropApp;
