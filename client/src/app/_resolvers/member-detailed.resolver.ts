import { inject } from '@angular/core';
import { ResolveFn } from '@angular/router';
import { MembersService } from '../_services/members.service';

export const memberDetailedResolver: ResolveFn<boolean> = (route, state) => {
  const memberServoce = inject(MembersService);

  return memberServoce.getMember(route.paramMap.get('username')!);
};
