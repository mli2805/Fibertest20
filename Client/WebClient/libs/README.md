# Known issues

If you get the `Uncaught ReferenceError: global is not defined` protobuf error, just define it somewhere as follows: 

`var global = global || window;`

# Usage

1. Download the `veex-<libName>-x.x.x.tgz` npm packages. For the sor viewer you need the following libs: `veex-common-x.x.x.tgz`, `veex-chart-x.x.x.tgz` and `veex-sor-x.x.x.tgz`. 
2. Install needed packages `npm install veex-<libName>-x.x.x.tgz --save`.
	npm install "libs\veex-common-0.0.15.tgz" --save
	npm install "libs\veex-chart-0.0.8.tgz" --save
	npm install "libs\veex-sor-0.0.32.tgz" --save
	
3. Install all `UNMET PEER DEPENDENCY`.
4. Wrap the `<app-root>` by the `vx-mouse-capture-wrapper` div. The div must be right after the `<body>` tag.

```html
<body>
  <div id="vx-mouse-capture-wrapper">
    <app-root></app-root>
  </div>
</body>
````

5. Import `VxSorViewerModule` modules.

```js
import { VxSorViewerModule} from '@veex/sor';

@NgModule({
  imports: [
    VxSorViewerModule
  ],
})
export class SomeModule { }

``` 

6. Add `<vx-sor-area>` to show sor and event table together or `<vx-sor-viewer>` to show only sor. Provide services for it. 

```js
import { Component } from '@angular/core';
import { SorViewerService, SorAreaViewerService, EventTableService } from '@veex/sor';
import { ChartMatrixesService, ChartDataService } from '@veex/chart';

@Component({
  selector: 'app-some-component',
  template: '<vx-sor-area></vx-sor-area>',
  providers: [SorAreaViewerService, SorViewerService, EventTableService, ChartDataService, ChartMatrixesService]
})
export class SomeComponent {
}

    NOTE: `SorAreaViewerService` service is a wrapper for `SorViewerService` and `EventTableService`. You can use it to call `setTraces` and to control edit states (`isEditMode`, `isDirty`).

```

7. Make sure the `<vx-sor-area>` has some width and height and use `SorAreaViewerService.setTraces(sorTraces: SorTraces[])`.
Show ( *ngIf ) the `<vx-sor-area>` only after `setTraces` was called.

    NOTE: you can find sor trace samples at `Fiberizer/project/test-stand/src/assets/samples`.

8. If you want to edit events you need to provide implementation of the `IDialogService` using the `VX_DIALOG_SERVICE` token to show a dialog from library.

 ```js
 import { VX_DIALOG_SERVICE } from '@veex/common';
  
  providers: [
    ...
    { provide: VX_DIALOG_SERVICE, useExisting: DialogService }
    ]
```

  Where `DialogService` is your implementation of the `IDialogService`.

    NOTE: Fiberizer VeEx libs are used by different applications with their own UI styles. That is why build-in dialog can't be used. 

    NOTE: Currently, a dialog appears only when last marker (end of fiber) is edited. An user is asked if he wants to ingore right markers location or not. 

You should proivde `IDialogService` implementation using dialog infrastructure of your app .

Here is the easiest sample which uses `confirm`:

```js
 import { IDialogService } from '@veex/common';

export class DialogService implements IDialogService {
  askYesNo(message: string): Promise<boolean> {
    const result = confirm(message);
    return Promise.resolve(result);
  }
}
```

9. Edit mode guide.

   To enter/leave edit mode use toggle the `SorAreaViewerService.isEditMode`. 

   Use `SorAreaViewService.isDirty` to check if user has edited any event. After changes are saved you should set the `isDirty` to `false`.
   
   Сall `SorAreaViewService.getApiSorChangeset()` to get the `ApiSorChangeset`. It contains all fields that can be changed during editing. Currently there are two of them:  `userOffset` and `keyEvents` array. Use the `ApiSorChangeset` to patch sor trace on the server side.



