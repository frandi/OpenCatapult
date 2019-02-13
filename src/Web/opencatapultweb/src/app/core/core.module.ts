import { NgModule, Optional, SkipSelf } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HTTP_INTERCEPTORS } from '@angular/common/http';
import { JwtInterceptor } from './interceptors/jwt.interceptor';
import { ErrorInterceptor } from './interceptors/error.interceptor';
import { ProjectService } from './services/project.service';
import { ApiService } from './services/api.service';
import { ProjectTemplateService } from './services/project-template.service';

@NgModule({
  declarations: [],
  imports: [
    CommonModule
  ],
  providers: [
    { provide: HTTP_INTERCEPTORS, useClass: JwtInterceptor, multi: true },
    { provide: HTTP_INTERCEPTORS, useClass: ErrorInterceptor, multi: true },
    ApiService,
    ProjectService,
    ProjectTemplateService
  ]
})
export class CoreModule { 
  constructor(@Optional() @SkipSelf() core: CoreModule) {
    if (core) {
      throw new Error('The core module is injected twice. It should only be injected in AppModule');
    }
  }
}
