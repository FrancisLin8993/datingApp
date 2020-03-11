import {
  BrowserModule,
  HammerGestureConfig,
  HAMMER_GESTURE_CONFIG
} from "@angular/platform-browser";
import { NgModule } from "@angular/core";
import { HttpClientModule } from "@angular/common/http";
import { FormsModule } from "@angular/forms";
import { BsDropdownModule, TabsModule } from "ngx-bootstrap";
import { BrowserAnimationsModule } from "@angular/platform-browser/animations";
import { RouterModule } from "@angular/router";
import { NgxGalleryModule } from "@kolkov/ngx-gallery";

import { AppComponent } from "./app.component";
import { NavComponent } from "./nav/nav.component";
import { HomeComponent } from "./home/home.component";
import { RegisterComponent } from "./register/register.component";
import { MemberListComponent } from "./members/member-list/member-list.component";
import { ListsComponent } from "./lists/lists.component";
import { MessagesComponent } from "./messages/messages.component";
import { MemberCardComponent } from "./members/member-list/member-card/member-card.component";

import { AuthService } from "./_services/auth.service";
import { UserService } from "./_services/user.service";

import { ErrorInterceptorProvider } from "./_services/error.interceptor";
import { MemberDetailResolver } from "./_resolvers/member-detail.resolver";

import { appRoutes } from "./routes";
import { JwtModule } from "@auth0/angular-jwt";
import { MemberDetailComponent } from "./members/member-list/member-detail/member-detail.component";
import { MemberListResolver } from "./_resolvers/member-list.resolver";
import { MemberEditComponent } from "./members/member-list/member-edit/member-edit.component";
import { MemberEditResolver } from "./_resolvers/member-edit.resolver";
import { PreventUnsavedChanges } from "./_guards/prevent-unsaved-changes.guard";

export function tokenGetter() {
  return localStorage.getItem("token");
}

// fix the ngx gallery bug
export class CustomHammerConfig extends HammerGestureConfig {
  overrides = {
    pinch: { enable: false },
    rotate: { enable: false }
  };
}

@NgModule({
  declarations: [
    AppComponent,
    NavComponent,
    HomeComponent,
    RegisterComponent,
    MemberListComponent,
    ListsComponent,
    MessagesComponent,
    MemberCardComponent,
    MemberDetailComponent,
    MemberEditComponent
  ],
  imports: [
    BrowserModule,
    HttpClientModule,
    BrowserAnimationsModule,
    FormsModule,
    BsDropdownModule.forRoot(),
    TabsModule.forRoot(),
    RouterModule.forRoot(appRoutes),
    NgxGalleryModule,
    JwtModule.forRoot({
      config: {
        tokenGetter,
        whitelistedDomains: ["localhost:5000"],
        blacklistedRoutes: ["localhost:5000/api/auth"]
      }
    })
  ],
  providers: [
    AuthService,
    UserService,
    ErrorInterceptorProvider,
    MemberDetailResolver,
    MemberListResolver,
    MemberEditResolver,
    PreventUnsavedChanges,
    {
      provide: HAMMER_GESTURE_CONFIG,
      useClass: CustomHammerConfig
    }
  ],
  bootstrap: [AppComponent]
})
export class AppModule {}
