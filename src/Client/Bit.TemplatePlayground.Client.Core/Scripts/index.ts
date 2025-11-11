
import './bswup';
import './theme';
import './events';
import { App } from './App';
import { WebInteropApp } from './WebInteropApp';

// Expose classes on window global
(window as any).App = App;
(window as any).WebInteropApp = WebInteropApp;
