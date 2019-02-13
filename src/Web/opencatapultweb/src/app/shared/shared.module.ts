import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TaskConfigFormComponent } from './components/task-config-form/task-config-form.component';

@NgModule({
  declarations: [TaskConfigFormComponent],
  imports: [
    CommonModule
  ],
  exports: [
    TaskConfigFormComponent
  ]
})
export class SharedModule { }
